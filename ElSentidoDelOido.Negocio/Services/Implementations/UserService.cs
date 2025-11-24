using AutoMapper;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Helpers;
using ElSentidoDelOido.Negocio.Services.Interfaces;

namespace ElSentidoDelOido.Negocio.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null) return null;
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> CreateAsync(UserCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password is required", nameof(dto.Password));

            var entity = _mapper.Map<User>(dto);
            entity.Password = Encrypt.HashPassword(dto.Password);
            entity.DateOfCreation = DateTime.UtcNow;

            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<UserDTO>(created);
        }

        public async Task<UserDTO> UpdateAsync(UserUpdateDTO dto)
        {
            var entity = await _repository.GetByIdAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException($"User with id {dto.Id} not found.");

            _mapper.Map(dto, entity);

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                entity.Password = Encrypt.HashPassword(dto.Password);
            }

            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<UserDTO>(updated);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<UserDTO?> AuthenticateAsync(string email, string password)
        {
            var userEntity = await _repository.GetByEmailAsync(email);
            if (userEntity == null) return null;

            if (!userEntity.Enabled.GetValueOrDefault())
                return null;

            try
            {
                var hashed = userEntity.Password ?? string.Empty;
                var verified = BCrypt.Net.BCrypt.Verify(password ?? string.Empty, hashed);
                if (!verified) return null;
            }
            catch
            {
                if (userEntity.Password != password) return null;
            }

            return _mapper.Map<UserDTO>(userEntity);
        }
    }
}
