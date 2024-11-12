using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static src.DTO.UserDTO;

namespace src.Services.user
{
    public interface IUserService
    {
        Task<UserReadDto> CreateOneAsync(UserCreateDto createDto);
        // get all
        Task<List<UserReadDto>> GetAllAsync();
        // get by id
        Task<UserReadDto> GetByIdAsync(Guid id);
        // get username by id
        Task<UserReadUsernameDto> GetUsernameByIdAsync(Guid id);
        // delete 
        Task<bool> DeleteOneAsync(Guid id);
        // update
        Task<bool> UpdateOneAsync(Guid id, UserUpdateDto updateDto);
        Task<string> SignInAsync(UserCreateDto createDto);

    }
}