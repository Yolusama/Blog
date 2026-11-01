namespace Blog.Domain.ArticleDomain.ValueObjects;

public class ArticleRankingVO
{
    const string DailyKey = "24小时内";
    const string WeeklyKey = "一周内";
    const string HalfMonthlyKey = "15天内";
    public struct ArticleRankingContent
    {
        public string ArticleId { get; set; }
        public string ArticleTitle { get; set; }
    }

   public Dictionary<string, IEnumerable<ArticleRankingContent>> Content { get; }

   public void SetDaily(IEnumerable<ArticleRankingContent> contents)
   {
       Content[DailyKey] = contents;
   }
   public  void SetWeekly(IEnumerable<ArticleRankingContent> contents)
   {
       Content[WeeklyKey] = contents;
   }

   public void SetHalfMonthly(IEnumerable<ArticleRankingContent> contents)
   {
       Content[HalfMonthlyKey] = contents;
   }
}