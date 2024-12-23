using Code_Record.Server.Models.MongoAtlas;
using Code_Record.Server.Models.MongoAtlas.DTOs;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Code_Record.Server.Extensions.Controllers
{
	public class PermissionService
	{
		private readonly IMongoCollection<Allow> _allowsCollection;
		private readonly IMongoCollection<Subject> _subjectsCollection;

		public PermissionService(
			IMongoCollection<Allow> allowsCollection,
			IMongoCollection<Subject> subjectsCollection)
		{
			_allowsCollection = allowsCollection;
			_subjectsCollection = subjectsCollection;
		}

		public async Task AddPermissions(IEnumerable<ObjectId> resourceIds, string resourceType, ObjectId? userId)
		{
			foreach (var resourceId in resourceIds)
			{
				var permission = new Allow
				{
					UserId = userId,
					ResourceId = resourceId,
					ResourceType = resourceType,
					CreationDate = DateTime.UtcNow,
					UpdateDate = null
				};

				await _allowsCollection.InsertOneAsync(permission);
			}
		}

		public async Task ProcessNestedPermissions<T>(
			IEnumerable<T> parents,
			Func<T, IEnumerable<ObjectId>> childSelector,
			string childResourceType,
			ObjectId? userId)
		{
			foreach (var parent in parents)
			{
				var childResourceIds = childSelector(parent)?.Where(id => id != ObjectId.Empty) ?? Enumerable.Empty<ObjectId>();
				await AddPermissions(childResourceIds, childResourceType, userId);
			}
		}

		public async Task AddPermissionsForSubject(ObjectId? subjectId, ObjectId? userId)
		{
			if (subjectId == null || subjectId == ObjectId.Empty)
			{
				throw new Exception("Invalid Subject ID.");
			}

			var subject = await _subjectsCollection.Find(s => s.Id == subjectId).FirstOrDefaultAsync();

			if (subject == null)
			{
				throw new Exception("Subject not found.");
			}

			// Add permissions to the Subject itself
			await AddPermissions(new[] { subjectId.Value }, "subject", userId);

			// Add permissions for Resources
			if (subject.Resources != null && subject.Resources.Any())
			{
				await AddPermissions(subject.Resources.Where(r => r != ObjectId.Empty), "resource", userId);
			}

			// Add permissions for Tests and their nested levels
			if (subject.Tests != null && subject.Tests.Any())
			{
				var testIds = subject.Tests.Select(t => t.Id).Where(id => id != ObjectId.Empty);
				await AddPermissions(testIds, "test", userId);

				await ProcessNestedPermissions(
					subject.Tests,
					test => test.Questions?.Select(q => q.Id).Where(id => id != ObjectId.Empty) ?? Enumerable.Empty<ObjectId>(),
					"question",
					userId
				);

				await ProcessNestedPermissions(
					subject.Tests.SelectMany(t => t.Questions ?? Enumerable.Empty<QuestionDTO>()),
					question => question.Options?.Where(o => o != ObjectId.Empty) ?? Enumerable.Empty<ObjectId>(),
					"option",
					userId
				);
			}

			// Add permissions for Themes and their Videos
			if (subject.Themes != null && subject.Themes.Any())
			{
				var themeIds = subject.Themes.Select(t => t.Id).Where(id => id != ObjectId.Empty);
				await AddPermissions(themeIds, "theme", userId);

				await ProcessNestedPermissions(
					subject.Themes,
					theme => theme.Videos?.Where(v => v != ObjectId.Empty) ?? Enumerable.Empty<ObjectId>(),
					"video",
					userId
				);
			}
		}

		// Método para recolectar IDs de recursos asociados a un Subject
		public async Task<HashSet<ObjectId>> CollectResourceIdsAsync(ObjectId subjectId)
		{
			var subject = await _subjectsCollection.Find(s => s.Id == subjectId).FirstOrDefaultAsync();

			if (subject == null)
			{
				throw new Exception("Subject not found.");
			}

			var resourceIds = new HashSet<ObjectId> { subjectId };

			if (subject.Resources != null)
			{
				resourceIds.UnionWith(subject.Resources);
			}

			if (subject.Tests != null)
			{
				var testIds = subject.Tests.Select(t => t.Id);
				resourceIds.UnionWith(testIds);

				var questionIds = subject.Tests
					.SelectMany(t => t.Questions ?? Enumerable.Empty<QuestionDTO>())
					.Select(q => q.Id);
				resourceIds.UnionWith(questionIds);

				var optionIds = subject.Tests
					.SelectMany(t => t.Questions ?? Enumerable.Empty<QuestionDTO>())
					.SelectMany(q => q.Options ?? Enumerable.Empty<ObjectId>());
				resourceIds.UnionWith(optionIds);
			}

			if (subject.Themes != null)
			{
				var themeIds = subject.Themes.Select(t => t.Id);
				resourceIds.UnionWith(themeIds);

				var videoIds = subject.Themes
					.SelectMany(t => t.Videos ?? Enumerable.Empty<ObjectId>());
				resourceIds.UnionWith(videoIds);
			}

			return resourceIds;
		}


		public async Task<FilterDefinition<Allow>> BuildDeleteFilterAsync(ObjectId subjectId, ObjectId userId)
		{
			var resourceIds = await CollectResourceIdsAsync(subjectId);

			return Builders<Allow>.Filter.And(
				Builders<Allow>.Filter.Eq("UserId", userId),
				Builders<Allow>.Filter.In("ResourceId", resourceIds)
			);
		}

	}
}
