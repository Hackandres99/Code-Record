using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;

namespace Code_Record.Server.Extensions.Controllers
{
	public static class MongoUpdater
	{

		public static async Task<bool> AddToListAsync<TDocument, TElement>(
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
