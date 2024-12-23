using Code_Record.Server.Models.SQLServer.DTOs;
using Code_Record.Server.Models.SQLServer.SubjectStructure;

namespace Code_Record.Server.Extensions.Controllers.DTOs
{
	public static class SubjectExtensions
	{
		public static IQueryable<SubjectDTO> ToSubjectDto(this IQueryable<Subject> query, string userEmail)
		{
			return query.Select(s => new SubjectDTO
			{
				Id = s.Id,
				Title = s.Title,
				Description = s.Description,
				Tool = s.Tool,
				Image = s.Image,
				Link = s.Link,
				CreationDate = s.CreationDate,
				UpdateDate = s.UpdateDate,
				Resources = s.Resources.Select(r => r.Id).ToList(),
				Tests = s.Tests.Select(t => new TestDTO
				{
					Id = t.Id,
					Questions = t.Questions.Select(q => new QuestionDTO
					{
						Id = q.Id,
						Options = q.Options.Select(o => o.Id).ToList()
					}).ToList(),
					Results = t.Results
					.Where(r => string.IsNullOrEmpty(userEmail) || r.UserEmail == userEmail)
					.Select(r => r.Id).ToList()
				}).ToList(),
				Themes = s.Themes.Select(t => new ThemeDTO
				{
					Id = t.Id,
					Videos = t.Videos.Select(v => new VideoDTO
					{
						Id = v.Id,
						Conversations = v.Conversations.Select(c => new ConversationDTO
						{
							Id = c.Id,
							Comments = c.Comments.Select(co => new CommentDTO
							{
								Id = co.Id,
								Anwsers = co.Anwsers.Select(a => a.Id).ToList()
							}).ToList()
						}).ToList()
					}).ToList()
				}).ToList()
			});
		}
	}
}
