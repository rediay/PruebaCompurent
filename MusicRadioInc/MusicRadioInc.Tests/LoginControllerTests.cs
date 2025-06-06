using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MusicRadioInc.Controllers;
using MusicRadioInc.Data;
using MusicRadioInc.Interfaces;
using MusicRadioInc.Models;

namespace MusicRadioInc.Tests
{
    public class LoginControllerTests
    {
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<DbSet<Client>> _mockDbSet;
        private List<Client> _usuarios;
        private LoginController _controller;
        private Mock<ISession> _mockSession;
        private Mock<IAuthService> _mockAuthService;

        public LoginControllerTests()
        {
            // Inicializar datos de prueba
            _usuarios = new List<Client>
            {
                new Client { Id = 1, UserLoginId = "user1", Password = BCrypt.Net.BCrypt.HashPassword("Pass123"), Name = "Usuario Uno", Mail = "user1@example.com" },
                new Client { Id = 2, UserLoginId = "admin", Password = BCrypt.Net.BCrypt.HashPassword("AdminPass"), Name = "Administrador", Mail = "admin@example.com" }
            };

            // Configurar DbSet mock
            _mockDbSet = new Mock<DbSet<Client>>();
            _mockDbSet.As<IQueryable<Client>>().Setup(m => m.Provider).Returns(_usuarios.AsQueryable().Provider);
            _mockDbSet.As<IQueryable<Client>>().Setup(m => m.Expression).Returns(_usuarios.AsQueryable().Expression);
            _mockDbSet.As<IQueryable<Client>>().Setup(m => m.ElementType).Returns(_usuarios.AsQueryable().ElementType);
            _mockDbSet.As<IQueryable<Client>>().Setup(m => m.GetEnumerator()).Returns(_usuarios.AsQueryable().GetEnumerator());

            // Configurar DbContext mock
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _mockContext.Setup(c => c.Clients).Returns(_mockDbSet.Object);

            // Simular operaciones de agregar y guardar cambios
            _mockDbSet.Setup(m => m.Add(It.IsAny<Client>())).Callback<Client>((s) => _usuarios.Add(s));
            _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            // Simular sesión con un diccionario interno
            var sessionData = new Dictionary<string, string>();
            _mockSession = new Mock<ISession>();
            _mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                        .Callback<string, byte[]>((key, value) => sessionData[key] = System.Text.Encoding.UTF8.GetString(value));
            _mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                        .Returns((string key, out byte[] value) =>
                        {
                            if (sessionData.ContainsKey(key))
                            {
                                value = System.Text.Encoding.UTF8.GetBytes(sessionData[key]);
                                return true;
                            }
                            value = null;
                            return false;
                        });
            _mockSession.Setup(s => s.Clear())
                        .Callback(() => sessionData.Clear());

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);

            // Inicializar el controlador
            _controller = new LoginController(_mockContext.Object, _mockAuthService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
        }

        // Prueba de Index (GET) cuando el usuario ya está logueado
        [Fact]
        public void Index_Get_UserAlreadyLoggedIn_RedirectsToHome()
        {
            // Arrange
            var sessionData = System.Text.Encoding.UTF8.GetBytes("user1");
            _mockSession.Setup(s => s.TryGetValue("UserLoginId", out sessionData)).Returns(true);

            // Act
            var result = _controller.Index();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
        }

        // Prueba de Index (GET) cuando el usuario no está logueado
        [Fact]
        public void Index_Get_UserNotLoggedIn_ReturnsView()
        {
            // Arrange
            byte[] sessionData = null; // Simula que no hay datos en la sesión
            _mockSession.Setup(s => s.TryGetValue("UserLoginId", out sessionData)).Returns(false);

            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // Prueba de Index (POST) con credenciales válidas
        [Fact]
        public async Task Index_Post_ValidCredentials_ReturnsRedirectToHome()
        {
            // Arrange
            var model = new LoginViewModel { UserLoginId = "user1", Password = "Pass123" };

            // Act
            var result = await _controller.Index(model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
            Assert.Equal("user1", _mockSession.Object.GetString("UserLoginId"));
        }

        // Prueba de Index (POST) con contraseña incorrecta
        [Fact]
        public async Task Index_Post_InvalidPassword_ReturnsViewWithModelError()
        {
            // Arrange
            var model = new LoginViewModel { UserLoginId = "user1", Password = "WrongPassword" };

            // Act
            var result = await _controller.Index(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("ID de usuario o contraseña incorrectos.", _controller.ModelState[string.Empty].Errors.Select(e => e.ErrorMessage));
        }

        // Prueba de Logout
        [Fact]
        public void Logout_ClearsSessionAndRedirectsToLogin()
        {
            // Act
            var result = _controller.Logout();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Login", redirectToActionResult.ControllerName);
            _mockSession.Verify(s => s.Clear(), Times.Once());
        }

        // Prueba de Register (GET)
        [Fact]
        public void Register_Get_ReturnsView()
        {
            // Act
            var result = _controller.Register();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // Prueba de Register (POST) con datos válidos
        [Fact]
        public async Task Register_Post_ValidUser_RedirectsToLogin()
        {
            // Arrange
            var newUser = new Client
            {
                UserLoginId = "newUser",
                Password = "NewPass123",
                Name = "Nuevo Usuario",
                Mail = "new@example.com"
            };

            // Act
            var result = await _controller.Register(newUser);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Login", redirectToActionResult.ControllerName);
            Assert.Contains(_usuarios, u => u.UserLoginId == newUser.UserLoginId);
        }

        // Prueba de Register (POST) con UserLoginId duplicado
        [Fact]
        public async Task Register_Post_DuplicateUserLoginId_ReturnsViewWithModelError()
        {
            // Arrange
            var existingUser = new Client { UserLoginId = "user1", Password = "AnotherPass", Mail = "dup@example.com" };

            // Act
            var result = await _controller.Register(existingUser);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Este ID de usuario ya está registrado.", _controller.ModelState["UserLoginId"].Errors.Select(e => e.ErrorMessage));
        }
    }
}