namespace Blog.MQ.Model.Enum;

public enum ArticlePublishStatus
{
    Published = 1,
    Auditing = 2,
    Audited = 3,
    NotPassed,
    Editing
}