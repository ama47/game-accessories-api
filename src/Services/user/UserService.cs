using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using src.Repository;
using src.Controllers;
using static src.DTO.UserDTO;
using src.Entity;
using src.Utils;
using static src.Entity.User;
using src.DTO;
using Microsoft.AspNetCore.Identity;


namespace src.Services.user
{
    public class UserService : IUserService
    {
        protected readonly UserRepository _userRepo;
        protected readonly IMapper _mapper;
        protected readonly IConfiguration _config;
        public UserService(UserRepository userRepo, IMapper mapper, IConfiguration config)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _config = config;
        }
        public async Task<UserReadDto> CreateOneAsync(UserCreateDto createDto)
        {
            var user = _mapper.Map<UserCreateDto, User>(createDto);
            var userTable = await _userRepo.GetAllAsync();
            if (userTable.Any(x => x.Email == user.Email))
            {
                throw CustomException.BadRequest("Email already registered please try another one");
            }
            if (userTable.Any(x => x.PhoneNumber == user.PhoneNumber))
            {
                throw CustomException.BadRequest("Phone number already registered please try another one");
            }
            if (userTable.Any(x => x.Username == user.Username))
            {
                throw CustomException.BadRequest("Username already registered please try another one");
            }
            if (user.Email == null)
            {
                throw CustomException.BadRequest("You cant leave Email empty");
            }
            else
            {
                if (user.Email.Contains("@admin.com"))
                {
                    user.Role = UserRole.Admin;
                }
                else
                {
                    user.Role = UserRole.Customer;
                }
            }
            if (user.PhoneNumber == null)
            {
                throw CustomException.BadRequest("You cant leave phone number empty");
            }
            if (user.Username == null)
            {
                throw CustomException.BadRequest("You cant leave Username empty");
            }
            if (user.FirstName == null)
            {
                throw CustomException.BadRequest("You cant leave First name empty");
            }
            if (user.LastName == null)
            {
                throw CustomException.BadRequest("You cant leave Last name empty");
            }
            if (user.BirthDate.Equals(DateOnly.Parse("0001-01-01")))
            {
                throw CustomException.BadRequest("You cant leave birthdate empty");
            }
            if (user.Password == null)
            {
                throw CustomException.BadRequest("You cant leave Password empty");
            }
            else
            {
                if (user.Password.Length < 8)
                {
                    throw CustomException.BadRequest("password should be at least 8 characters");
                }
                else if ((!user.Password.Contains("1")) && (!user.Password.Contains("2")) && (!user.Password.Contains("3")) && (!user.Password.Contains("4")) && (!user.Password.Contains("5")) && (!user.Password.Contains("6")) && (!user.Password.Contains("7")) && (!user.Password.Contains("8")) && (!user.Password.Contains("9")) && (!user.Password.Contains("0")))
                {
                    throw CustomException.BadRequest("password should contains at least one number");
                }
                else if ((!user.Password.Contains("!")) && (!user.Password.Contains("@")) && (!user.Password.Contains("#")) && (!user.Password.Contains("$")) && (!user.Password.Contains("%")) && (!user.Password.Contains("^")) && (!user.Password.Contains("&")) && (!user.Password.Contains("*")) && (!user.Password.Contains("(")) && (!user.Password.Contains(")")) && (!user.Password.Contains("_")) && (!user.Password.Contains("[")) && (!user.Password.Contains("")))
                {
                    throw CustomException.BadRequest("password should contains at least one special character (! - @ - # - $ - % - & - * - ( - ) - _ - [ - ])");
                }
            }
            PasswordUtils.HashPassword(createDto.Password, out string hashedPassword, out byte[] salt);
            user.CartId = Guid.NewGuid();
            user.Password = hashedPassword;
            user.Salt = salt;
            var savedUser = await _userRepo.CreateOneAsync(user);
            return _mapper.Map<User, UserReadDto>(savedUser);
        }

        //sign in
        public async Task<string> SignInAsync(UserCreateDto createDto)
        {
            // logic
            // find user by Email
            var foundUser = await _userRepo.FindByEmailAsync(createDto.Email);
            // check password
            var isMatched = PasswordUtils.VerifyPassword(createDto.Password, foundUser.Password, foundUser.Salt);
            if (isMatched)
            {
                // create token 
                var tokenUtil = new TokenUtils(_config);
                return tokenUtil.GenerateToken(foundUser);
            }
            // string
            throw CustomException.UnAuthorized($"user with {foundUser.Email} password doesnt match");
        }
        // get by id
        public async Task<UserReadDto> GetByIdAsync(Guid id)
        {
            var foundUser = await _userRepo.GetByIdAsync(id);
            return _mapper.Map<User, UserReadDto>(foundUser);
        }
        // get by id
        public async Task<UserReadUsernameDto> GetUsernameByIdAsync(Guid id)
        {
            var foundUser = await _userRepo.GetByIdAsync(id);
            return _mapper.Map<User, UserReadUsernameDto>(foundUser);
        }
        // delete 
        public async Task<bool> DeleteOneAsync(Guid id)
        {
            var foundUser = await _userRepo.GetByIdAsync(id);
            bool isDeleted = await _userRepo.DeleteOneAsync(foundUser);
            if (isDeleted)
            {
                return true;
            }
            return false;
        }
        // update
        public async Task<bool> UpdateOneAsync(Guid id, UserUpdateDto updateDto)
        {
            var foundUser = await _userRepo.GetByIdAsync(id);
            var userTable = await _userRepo.GetAllAsync();
            var duplicatEmail = userTable.Any(x => x.Email == updateDto.Email && x.UserId != foundUser.UserId);
            var duplicatUsername = userTable.Any(x => x.Username == updateDto.Username && x.UserId != foundUser.UserId);
            var duplicatPhone = userTable.Any(x => x.PhoneNumber == updateDto.PhoneNumber && x.UserId != foundUser.UserId);
            if (duplicatEmail)
            {
                throw CustomException.BadRequest($"email already exist try another one");
            }
            if (duplicatUsername)
            {
                throw CustomException.BadRequest($"Username already exist try another one");
            }
            if (duplicatPhone)
            {
                throw CustomException.BadRequest($"phone number already exist try another one");
            }
            if (foundUser == null)
            {
                throw CustomException.BadRequest($"user with {id}  doesnt exist");
            }
            else
            {
                if (updateDto.Email == null)
                {
                    updateDto.Email = foundUser.Email;
                }
                if (updateDto.Username == null)
                {
                    updateDto.Username = foundUser.Username;
                }
                if (updateDto.FirstName == null)
                {
                    updateDto.FirstName = foundUser.FirstName;
                }
                if (updateDto.LastName == null)
                {
                    updateDto.LastName = foundUser.LastName;
                }
                if (updateDto.PhoneNumber == null)
                {
                    updateDto.PhoneNumber = foundUser.PhoneNumber;
                }
                if (updateDto.Password == null)
                {
                    updateDto.Password = foundUser.Password;
                }
                else
                {
                    if (updateDto.Password.Length < 8)
                    {
                        throw CustomException.BadRequest("password should be at least 8 characters");
                    }
                    else if ((!updateDto.Password.Contains("1")) && (!updateDto.Password.Contains("2")) && (!updateDto.Password.Contains("3")) && (!updateDto.Password.Contains("4")) && (!updateDto.Password.Contains("5")) && (!updateDto.Password.Contains("6")) && (!updateDto.Password.Contains("7")) && (!updateDto.Password.Contains("8")) && (!updateDto.Password.Contains("9")) && (!updateDto.Password.Contains("0")))
                    {
                        throw CustomException.BadRequest("password should contains at least one number");
                    }
                    else if ((!updateDto.Password.Contains("!")) && (!updateDto.Password.Contains("@")) && (!updateDto.Password.Contains("#")) && (!updateDto.Password.Contains("$")) && (!updateDto.Password.Contains("%")) && (!updateDto.Password.Contains("^")) && (!updateDto.Password.Contains("&")) && (!updateDto.Password.Contains("*")) && (!updateDto.Password.Contains("(")) && (!updateDto.Password.Contains(")")) && (!updateDto.Password.Contains("_")) && (!updateDto.Password.Contains("[")) && (!updateDto.Password.Contains("]")))
                    {
                        throw CustomException.BadRequest("password should contains at least one special charachter (! - @ - # - $ - % - & - * - ( - ) - _ - [ - ])");
                    }
                }
                if (updateDto.CartId == null)
                {
                    updateDto.CartId = foundUser.CartId;
                }
                if (foundUser.Email.Contains("@admin.com"))
                {
                    updateDto.Role = UserRole.Admin;
                }
                else
                {
                    updateDto.Role = UserRole.Customer;
                }
                if (updateDto.BirthDate.Equals(DateOnly.Parse("0001-01-01")))
                {
                    updateDto.BirthDate = foundUser.BirthDate;
                }
                _mapper.Map(updateDto, foundUser);
                PasswordUtils.HashPassword(foundUser.Password, out string hashedPassword, out byte[] salt);
                foundUser.Password = hashedPassword;
                foundUser.Salt = salt;
                return await _userRepo.UpdateOneAsync(foundUser);
            }

        }
        public async Task<List<UserReadDto>> GetAllAsync()
        {
            var UserList = await _userRepo.GetAllAsync();
            return _mapper.Map<List<User>, List<UserReadDto>>(UserList);
        }
    }
}