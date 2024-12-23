using Code_Record.Server.Models.MongoAtlas.SubjectStructure;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure;
using Code_Record.Server.Models.MongoAtlas.DTOs;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;

namespace Code_Record.Server.Extensions.Controllers
{
	public class MongoPostService
	{
		private readonly IMongoCollection<Option> _optionsCollection;
		private readonly IMongoCollection<Question> _questionsCollection;
		private readonly IMongoCollection<Test> _testsCollection;
		private readonly IMongoCollection<Video> _videosCollection;
		private readonly IMongoCollection<Theme> _themesCollection;
		private readonly IMongoCollection<Resource> _resourcesCollection;
		private readonly IMongoCollection<Subject> _subjectsCollection;
		

		public MongoPostService(
			IMongoCollection<Option> optionsCollection,
			IMongoCollection<Question> questionsCollection,
			IMongoCollection<Test> testsCollection,
			IMongoCollection<Video> videosCollection,
			IMongoCollection<Theme> themesCollection,
			IMongoCollection<Resource> resourcesCollection,
			IMongoCollection<Subject> subjectsCollection
			)
		{
			_optionsCollection = optionsCollection;
			_questionsCollection = questionsCollection;
			_testsCollection = testsCollection;
			_videosCollection = videosCollection;
			_themesCollection = themesCollection;
			_resourcesCollection = resourcesCollection;
			_subjectsCollection = subjectsCollection;
		}

		public async Task<string> CreateOptionAsync(Option option)
		{
			option.CreationDate = DateTime.UtcNow;
			option.UpdateDate = null;
			await _optionsCollection.InsertOneAsync(option);

			var optionAddedToQuestion = await AddToListAsync(
				_questionsCollection,
				option.IdQuestion,
				q => q.Id == option.IdQuestion,
				q => q.Options,
				option.Id
			);

			if (!optionAddedToQuestion) return "Question not found.";

			var question = await _questionsCollection.Find(q => q.Id == option.IdQuestion).FirstOrDefaultAsync();
			if (question?.IdTest != null)
			{
				var optionAddedToTest = await AddToListAsync(
					_testsCollection,
					question.IdTest,
					t => t.Id == question.IdTest,
					t => t.Questions.First(q => q.Id == question.Id).Options,
					option.Id
				);

				if (!optionAddedToTest) return "Test not found.";

				var test = await _testsCollection.Find(t => t.Id == question.IdTest).FirstOrDefaultAsync();
				if (test?.IdSubject != null)
				{
					await AddToListAsync(
						_subjectsCollection,
						test.IdSubject,
						s => s.Id == test.IdSubject,
						s => s.Tests.First(t => t.Id == test.Id).Questions.First(q => q.Id == question.Id).Options,
						option.Id
					);
				}
			}

			return "";
		}

		public async Task<string> CreateQuestionAsync(Question question)
		{
			question.CreationDate = DateTime.UtcNow;
			question.UpdateDate = null;
			await _questionsCollection.InsertOneAsync(question);

			var questionDTO = new QuestionDTO
			{
				Id = question.Id,
				Options = question.Options ?? new List<ObjectId>()
			};

			var testUpdated = await AddToListAsync(
				_testsCollection,
				question.IdTest,
				t => t.Id == question.IdTest,
				t => t.Questions,
				questionDTO
			);

			if (!testUpdated) return "Test not found.";

			var test = await _testsCollection.Find(t => t.Id == question.IdTest).FirstOrDefaultAsync();
			if (test?.IdSubject != null)
			{
				await AddToListAsync(
					_subjectsCollection,
					test.IdSubject,
					s => s.Id == test.IdSubject,
					s => s.Tests.First(t => t.Id == test.Id).Questions,
					questionDTO
				);
			}

			return "";
		}

		public async Task<string> CreateTestAsync(Test test)
		{
			test.CreationDate = DateTime.UtcNow;
			test.UpdateDate = null;
			await _testsCollection.InsertOneAsync(test);

			var added = await AddToListAsync(
				_subjectsCollection,
				test.IdSubject,
				s => s.Id == test.IdSubject,
				s => s.Tests,
				new TestDTO
				{
					Id = test.Id,
					Questions = test.Questions ?? new List<QuestionDTO>()
				}
			);

			if (!added) return "Could not associate test with the subject.";

			return "";
		}

		public async Task<string> CreateVideoAsync(Video video)
		{
			video.UploadDate = DateTime.UtcNow;
			video.UpdateDate = null;
			await _videosCollection.InsertOneAsync(video);

			var themeUpdated = await AddToListAsync(
				_themesCollection,
				video.IdTheme,
				t => t.Id == video.IdTheme,
				t => t.Videos,
				video.Id
			);

			if (!themeUpdated) return "Theme not found.";

			var theme = await _themesCollection.Find(t => t.Id == video.IdTheme).FirstOrDefaultAsync();
			if (theme?.IdSubject != null)
			{
				var subjectUpdated = await AddToListAsync(
					_subjectsCollection,
					theme.IdSubject,
					s => s.Id == theme.IdSubject,
					s => s.Themes.First(t => t.Id == theme.Id).Videos,
					video.Id
				);

				if (!subjectUpdated) return "Failed to add video to subject.";
			}

			return "";
		}

		public async Task<string> CreateThemeAsync(Theme theme)
		{
			theme.CreationDate = DateTime.UtcNow;
			theme.UpdateDate = null;
			await _themesCollection.InsertOneAsync(theme);

			var added = await AddToListAsync(
				_subjectsCollection,
				theme.IdSubject,
				s => s.Id == theme.IdSubject,
				s => s.Themes,
				new ThemeDTO
				{
					Id = theme.Id,
					Videos = theme.Videos ?? new List<ObjectId>()
				}
			);

			if (!added) return "Could not associate theme with the subject.";

			return "";
		}

		public async Task<string> CreateResourceAsync(Resource resource)
		{
			resource.UploadDate = DateTime.UtcNow;
			resource.UpdateDate = null;
			await _resourcesCollection.InsertOneAsync(resource);

			var added = await MongoUpdater.AddToListAsync(
				_subjectsCollection,
				resource.IdSubject,
				s => s.Id == resource.IdSubject,
				s => s.Resources,
				resource.Id
			);

			if (!added) return "Could not associate resource with the subject.";

			return "";
		}

		private static async Task<bool> AddToListAsync<TDocument, TElement>(
			IMongoCollection<TDocument> collection,
			ObjectId? parentId,
			Expression<Func<TDocument, bool>> filter,
			Expression<Func<TDocument, IEnumerable<TElement>>> listSelector,
			TElement newItem
		)
		where TDocument : class
		{
			if (parentId == null) return false;

			var parent = await collection.Find(filter).FirstOrDefaultAsync();
			if (parent == null) return false;

			var listProperty = listSelector.Compile().Invoke(parent);
			if (listProperty == null)
			{
				var accessor = (listSelector.Body as MemberExpression)?.Member;
				if (accessor is PropertyInfo propertyInfo)
				{
					propertyInfo.SetValue(parent, new List<TElement>());
					listProperty = propertyInfo.GetValue(parent) as IEnumerable<TElement>;
				}
			}

			(listProperty as ICollection<TElement>)?.Add(newItem);

			await collection.ReplaceOneAsync(filter, parent);

			return true;
		}
	}
}
