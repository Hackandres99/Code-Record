using Code_Record.Server.Models.MongoAtlas.SubjectStructure;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Code_Record.Server.Extensions.Controllers
{
	public class MongoDeleteService
	{
		private readonly IMongoCollection<Subject> _subjectsCollection;
		private readonly IMongoCollection<Theme> _themesCollection;
		private readonly IMongoCollection<Resource> _resourcesCollection;
		private readonly IMongoCollection<Video> _videosCollection;
		private readonly IMongoCollection<Test> _testsCollection;
		private readonly IMongoCollection<Question> _questionsCollection;
		private readonly IMongoCollection<Option> _optionsCollection;

		public MongoDeleteService(
			IMongoCollection<Subject> subjectsCollection,
			IMongoCollection<Theme> themesCollection,
			IMongoCollection<Resource> resourcesCollection,
			IMongoCollection<Video> videosCollection,
			IMongoCollection<Test> testsCollection,
			IMongoCollection<Question> questionsCollection,
			IMongoCollection<Option> optionsCollection
		)
		{
			_subjectsCollection = subjectsCollection;
			_themesCollection = themesCollection;
			_resourcesCollection = resourcesCollection;
			_videosCollection = videosCollection;
			_testsCollection = testsCollection;
			_questionsCollection = questionsCollection;
			_optionsCollection = optionsCollection;
		}
	
		public async Task<string> SubjectsAsync(string subjectId)
		{
			if (!ObjectId.TryParse(subjectId, out var parsedSubjectId))
				return "Invalid Subject ID format.";

			// Buscar el Subject
			var subject = await _subjectsCollection.Find(s => s.Id == parsedSubjectId).FirstOrDefaultAsync();
			if (subject == null) return "Subject not found.";

			// Eliminar Videos relacionados con el Subject
			if (subject?.Themes != null)
			{
				foreach (var themeDto in subject.Themes)
				{
					var theme = await _themesCollection.Find(t => t.Id == themeDto.Id).FirstOrDefaultAsync();
					if (theme != null)
					{
						// Eliminar Videos relacionados con el Theme
						if (theme.Videos != null)
						{
							await _videosCollection.DeleteManyAsync(v => v.IdTheme == theme.Id);
						}

						// Eliminar el Theme
						await _themesCollection.DeleteOneAsync(t => t.Id == theme.Id);
					}
				}
			}

			// Eliminar Resources relacionados con el Subject
			if (subject?.Resources != null)
			{
				await _resourcesCollection.DeleteManyAsync(r => subject.Resources.Contains(r.Id));
			}

			// Eliminar Tests relacionados con el Subject
			if (subject?.Tests != null)
			{
				foreach (var testDto in subject.Tests)
				{
					var test = await _testsCollection.Find(t => t.Id == testDto.Id).FirstOrDefaultAsync();
					if (test != null)
					{
						// Eliminar Questions y Options asociados a los Tests
						if (test.Questions != null)
						{
							foreach (var questionDto in test.Questions)
							{
								var question = await _questionsCollection.Find(q => q.Id == questionDto.Id).FirstOrDefaultAsync();
								if (question != null && question.Options != null)
								{
									await _optionsCollection.DeleteManyAsync(o => question.Options.Contains(o.Id));
								}
								await _questionsCollection.DeleteOneAsync(q => q.Id == questionDto.Id);
							}
						}
						await _testsCollection.DeleteOneAsync(t => t.Id == testDto.Id);
					}
				}
			}

			// Eliminar el Subject
			var result = await _subjectsCollection.DeleteOneAsync(s => s.Id == parsedSubjectId);
			if (result.DeletedCount == 0)
				return "Failed to delete the subject.";

			return "";
		}

		public async Task<string> ResourceAsync(string resourceId)
		{
			if (!ObjectId.TryParse(resourceId, out var parsedResourceId))
				return "Invalid Resource ID format."; // ID inválido

			// Buscar el recurso en la base de datos
			var resource = await _resourcesCollection.Find(r => r.Id == parsedResourceId).FirstOrDefaultAsync();
			if (resource == null)
				return "Resource not found."; // Recurso no encontrado

			// Eliminar el recurso de la colección
			var deleteResult = await _resourcesCollection.DeleteOneAsync(r => r.Id == parsedResourceId);
			if (deleteResult.DeletedCount == 0)
				return "Failed to delete the resource."; // No se eliminó el recurso

			// Actualizar el Subject eliminando la referencia al recurso
			if (resource.IdSubject != null)
			{
				var subject = await _subjectsCollection.Find(s => s.Id == resource.IdSubject).FirstOrDefaultAsync();
				if (subject != null && subject.Resources != null)
				{
					// Remover el recurso de la lista de recursos
					subject.Resources = subject.Resources.Where(r => r != parsedResourceId).ToList();

					// Actualizar el Subject en la base de datos
					var subjectUpdateResult = await _subjectsCollection.ReplaceOneAsync(
						s => s.Id == subject.Id,
						subject
					);

					if (!subjectUpdateResult.IsAcknowledged || subjectUpdateResult.ModifiedCount == 0)
						return $"Failed to update subject {subject.Id} after deleting resource {resource.Id}."; // No se pudo actualizar el Subject
				}
			}

			return ""; // Todo salió bien
		}

		public async Task<string> ThemeAsync(string themeId)
		{
			if (!ObjectId.TryParse(themeId, out var parsedThemeId))
				return "Invalid Theme ID format.";

			// Buscar el tema en la base de datos
			var theme = await _themesCollection.Find(t => t.Id == parsedThemeId).FirstOrDefaultAsync();
			if (theme == null)
				return "Theme not found.";

			// Eliminar los videos asociados al tema
			if (theme.Videos != null && theme.Videos.Any())
			{
				var videoDeleteResult = await _videosCollection.DeleteManyAsync(v => theme.Videos.Contains(v.Id));
				if (!videoDeleteResult.IsAcknowledged)
					return $"Failed to delete videos associated with theme {theme.Id}.";
			}

			// Eliminar el tema de la colección
			var themeDeleteResult = await _themesCollection.DeleteOneAsync(t => t.Id == parsedThemeId);
			if (themeDeleteResult.DeletedCount == 0)
				return "Failed to delete the theme.";

			// Actualizar el Subject eliminando la referencia al tema
			if (theme.IdSubject != null)
			{
				var subject = await _subjectsCollection.Find(s => s.Id == theme.IdSubject).FirstOrDefaultAsync();
				if (subject != null && subject.Themes != null)
				{
					subject.Themes = subject.Themes.Where(t => t.Id != parsedThemeId).ToList();
					var subjectUpdateResult = await _subjectsCollection.ReplaceOneAsync(
						s => s.Id == subject.Id,
						subject
					);

					if (!subjectUpdateResult.IsAcknowledged || subjectUpdateResult.ModifiedCount == 0)
						return $"Failed to update subject {subject.Id} after deleting theme {theme.Id}.";
				}
			}

			return "";
		}

		public async Task<string> VideoAsync(string videoId)
		{
			if (!ObjectId.TryParse(videoId, out var parsedVideoId))
				return "Invalid Video ID format.";

			// Buscar el video en la base de datos
			var video = await _videosCollection.Find(v => v.Id == parsedVideoId).FirstOrDefaultAsync();
			if (video == null)
				return "Video not found.";

			// Eliminar el video de la colección
			var deleteResult = await _videosCollection.DeleteOneAsync(v => v.Id == parsedVideoId);
			if (deleteResult.DeletedCount == 0)
				return "Failed to delete the video.";

			// Actualizar el tema eliminando la referencia al video
			var theme = await _themesCollection.Find(t => t.Id == video.IdTheme).FirstOrDefaultAsync();
			if (theme != null && theme.Videos != null)
			{
				theme.Videos = theme.Videos.Where(vidId => vidId != parsedVideoId).ToList();
				var themeUpdateResult = await _themesCollection.ReplaceOneAsync(
					t => t.Id == theme.Id,
					theme
				);

				if (!themeUpdateResult.IsAcknowledged || themeUpdateResult.ModifiedCount == 0)
					return $"Failed to update theme {theme.Id} after deleting video {videoId}.";
			}

			// Actualizar el Subject eliminando la referencia al video
			if (theme?.IdSubject != null)
			{
				var subject = await _subjectsCollection.Find(s => s.Id == theme.IdSubject).FirstOrDefaultAsync();
				if (subject != null)
				{
					var subjectTheme = subject.Themes?.FirstOrDefault(t => t.Id == theme.Id);
					if (subjectTheme?.Videos != null)
					{
						subjectTheme.Videos = subjectTheme.Videos.Where(vidId => vidId != parsedVideoId).ToList();
						var subjectUpdateResult = await _subjectsCollection.ReplaceOneAsync(
							s => s.Id == subject.Id,
							subject
						);

						if (!subjectUpdateResult.IsAcknowledged || subjectUpdateResult.ModifiedCount == 0)
							return $"Failed to update subject {subject.Id} after deleting video {videoId}.";
					}
				}
			}

			return "";
		}

		public async Task<string> TestAsync(string testId)
		{
			if (!ObjectId.TryParse(testId, out var parsedTestId))
				return "Invalid Test ID format.";

			// Buscar el Test en la base de datos
			var test = await _testsCollection.Find(t => t.Id == parsedTestId).FirstOrDefaultAsync();
			if (test == null)
				return "Test not found.";

			// Eliminar las preguntas asociadas al Test
			if (test.Questions != null && test.Questions.Any())
			{
				foreach (var question in test.Questions)
				{
					// Eliminar las opciones asociadas a la pregunta
					if (question.Options != null && question.Options.Any())
					{
						var deleteOptionsResult = await _optionsCollection.DeleteManyAsync(o => question.Options.Contains(o.Id));
						if (!deleteOptionsResult.IsAcknowledged)
							return $"Failed to delete options associated with question {question.Id}.";
					}

					// Eliminar la pregunta
					var deleteQuestionResult = await _questionsCollection.DeleteOneAsync(q => q.Id == question.Id);
					if (deleteQuestionResult.DeletedCount == 0)
						return $"Failed to delete question {question.Id} from test {testId}.";
				}
			}

			// Eliminar el Test de la base de datos
			var deleteTestResult = await _testsCollection.DeleteOneAsync(t => t.Id == parsedTestId);
			if (deleteTestResult.DeletedCount == 0)
				return "Failed to delete the test.";

			// Actualizar el Subject eliminando la referencia al Test
			if (test.IdSubject != null)
			{
				var subject = await _subjectsCollection.Find(s => s.Id == test.IdSubject).FirstOrDefaultAsync();
				if (subject?.Tests != null)
				{
					subject.Tests = subject.Tests.Where(t => t.Id != test.Id).ToList();
					var subjectUpdateResult = await _subjectsCollection.ReplaceOneAsync(
						s => s.Id == subject.Id,
						subject
					);

					if (!subjectUpdateResult.IsAcknowledged || subjectUpdateResult.ModifiedCount == 0)
						return $"Failed to update subject {test.IdSubject} after deleting test {testId}.";
				}
			}

			return "";
		}

		public async Task<string> QuestionAsync(string questionId)
		{
			if (!ObjectId.TryParse(questionId, out var parsedQuestionId))
				return "Invalid Question ID format.";

			// Buscar la pregunta en la base de datos
			var question = await _questionsCollection.Find(q => q.Id == parsedQuestionId).FirstOrDefaultAsync();
			if (question == null)
				return "Question not found.";

			// Eliminar las opciones asociadas a la pregunta
			if (question.Options != null && question.Options.Any())
			{
				var deleteOptionsResult = await _optionsCollection.DeleteManyAsync(o => question.Options.Contains(o.Id));
				if (!deleteOptionsResult.IsAcknowledged)
					return "Failed to delete options associated with the question.";
			}

			// Eliminar la pregunta de la colección
			var deleteResult = await _questionsCollection.DeleteOneAsync(q => q.Id == parsedQuestionId);
			if (deleteResult.DeletedCount == 0)
				return "Failed to delete the question.";

			// Actualizar el Test eliminando la referencia al Question
			if (question.IdTest != null)
			{
				var test = await _testsCollection.Find(t => t.Id == question.IdTest).FirstOrDefaultAsync();
				if (test != null && test.Questions != null)
				{
					test.Questions = test.Questions.Where(q => q.Id != parsedQuestionId).ToList();
					var testUpdateResult = await _testsCollection.ReplaceOneAsync(
						t => t.Id == test.Id,
						test
					);

					if (!testUpdateResult.IsAcknowledged || testUpdateResult.ModifiedCount == 0)
						return $"Failed to remove question {questionId} from test {question.IdTest}";

					// Actualizar el Subject si el Test está asociado a un Subject
					if (test.IdSubject != null)
					{
						var subject = await _subjectsCollection.Find(s => s.Id == test.IdSubject).FirstOrDefaultAsync();
						if (subject != null && subject.Tests != null)
						{
							var subjectTest = subject.Tests.FirstOrDefault(t => t.Id == test.Id);
							if (subjectTest != null)
							{
								subjectTest.Questions = test.Questions;
								var subjectUpdateResult = await _subjectsCollection.ReplaceOneAsync(
									s => s.Id == test.IdSubject,
									subject
								);

								if (!subjectUpdateResult.IsAcknowledged || subjectUpdateResult.ModifiedCount == 0)
									return $"Failed to update subject {test.IdSubject} with updated test questions.";
							}
						}
					}
				}
			}

			return "";
		}

		public async Task<string> OptionAsync(string optionId)
		{
			if (!ObjectId.TryParse(optionId, out var parsedOptionId))
				return "Invalid Option ID format.";

			// Buscar la opción en la base de datos
			var option = await _optionsCollection.Find(o => o.Id == parsedOptionId).FirstOrDefaultAsync();
			if (option == null)
				return "Option not found.";

			// Eliminar la opción de la colección
			var deleteResult = await _optionsCollection.DeleteOneAsync(o => o.Id == parsedOptionId);
			if (deleteResult.DeletedCount == 0)
				return "Failed to delete the option.";

			// Actualizar la pregunta eliminando la referencia a la opción
			var question = await _questionsCollection.Find(q => q.Id == option.IdQuestion).FirstOrDefaultAsync();
			if (question != null && question.Options != null)
			{
				question.Options = question.Options.Where(optId => optId != parsedOptionId).ToList();
				var questionUpdateResult = await _questionsCollection.ReplaceOneAsync(
					q => q.Id == question.Id,
					question
				);

				if (!questionUpdateResult.IsAcknowledged || questionUpdateResult.ModifiedCount == 0)
					return $"Failed to update question {question.Id} after deleting option {optionId}.";
			}

			// Actualizar el test eliminando la referencia a la opción
			if (question?.IdTest != null)
			{
				var test = await _testsCollection.Find(t => t.Id == question.IdTest).FirstOrDefaultAsync();
				if (test != null)
				{
					var testQuestion = test.Questions?.FirstOrDefault(q => q.Id == question.Id);
					if (testQuestion?.Options != null)
					{
						testQuestion.Options = testQuestion.Options.Where(optId => optId != parsedOptionId).ToList();
						var testUpdateResult = await _testsCollection.ReplaceOneAsync(
							t => t.Id == test.Id, test
						);

						if (!testUpdateResult.IsAcknowledged || testUpdateResult.ModifiedCount == 0)
							return $"Failed to update test {test.Id} after deleting option {optionId}.";
					}

					// Actualizar el Subject eliminando la referencia a la opción
					if (test.IdSubject != null)
					{
						var subject = await _subjectsCollection.Find(s => s.Id == test.IdSubject).FirstOrDefaultAsync();
						if (subject != null)
						{
							var subjectTest = subject.Tests?.FirstOrDefault(t => t.Id == test.Id);
							var subjectQuestion = subjectTest?.Questions?.FirstOrDefault(q => q.Id == question.Id);

							if (subjectQuestion?.Options != null)
							{
								subjectQuestion.Options = subjectQuestion.Options.Where(optId => optId != parsedOptionId).ToList();
								var subjectUpdateResult = await _subjectsCollection.ReplaceOneAsync(
									s => s.Id == subject.Id, subject
								);

								if (!subjectUpdateResult.IsAcknowledged || subjectUpdateResult.ModifiedCount == 0)
									return $"Failed to update subject {subject.Id} after deleting option {optionId}.";
							}
						}
					}
				}
			}
			return "";
		}

	}

}
