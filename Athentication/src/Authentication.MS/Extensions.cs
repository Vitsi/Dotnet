using Authentication.Ms.Models;

namespace Authentication.Ms
{
    public static class Extensions
    {
        public static UserDto AsDto(this UserModel user)
        {
            return new UserDto (user.Id,
                            user.Username,
                            user.EmailAddress,
                            user.Role,
                            user.CreatedDate,
                            user.Photo
                           );
        }
    }
}