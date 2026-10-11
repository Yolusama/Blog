namespace Blog.Domain.UserDomain.ValueObjects;

public struct UserJwtInfo
{
   public int UserId { get; set; }

   public UserJwtInfo(int userId)
   {
      UserId = userId;
   }
}