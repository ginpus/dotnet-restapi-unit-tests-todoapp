using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Controllers;
using Xunit;

namespace RestAPI.UnitTests.Controllers
{
    public class TodosController_Should
    {
        private readonly Random _random = new Random();

        // UnitOfWork_StateUnderTest_ExpectedBehavior
        [Fact]
        public async Task GetAllTodoItems_When_GetAllIsCalled()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var usersRepositoryMock = new Mock<IUserRepository>();
            var todosRepositoryMock = new Mock<ITodosRepository>();

            var httpContextMock = new Mock<HttpContext>();

            httpContextMock.SetupGet(x => x.Items["userId"]).Returns(userId);

            var expectedItems = new List<TodoItemReadModel>
            {
                GenerateTodoItems(),
                GenerateTodoItems(),
                GenerateTodoItems()
            };

            expectedItems.ForEach(todoItem => todoItem.UserId = userId);

            todosRepositoryMock.Setup(x => x.GetAllAsync(userId)).ReturnsAsync(expectedItems);

            var sut = new TodosController(todosRepositoryMock.Object, usersRepositoryMock.Object)
            {
                ControllerContext =
                {
                    HttpContext = httpContextMock.Object
                }
            };
            
            // Act
            var result = await sut.GetAll();

            // Assert
            result.Should().BeEquivalentTo(expectedItems, options => options.ComparingByMembers<TodoItemReadModel>());
            
            todosRepositoryMock.Verify(x => x.GetAllAsync(userId), Times.Once);
        }

        private TodoItemReadModel GenerateTodoItems()
        {
            return new TodoItemReadModel
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Difficulty = (Difficulty)_random.Next(0, 4),
                IsDone = false,
                DateCreated = DateTime.Now
            };
        }
    }
}