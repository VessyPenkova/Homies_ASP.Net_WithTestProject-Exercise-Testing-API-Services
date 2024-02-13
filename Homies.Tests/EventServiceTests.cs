using Homies.Data;
using Homies.Data.Models;
using Homies.Models.Event;
using Homies.Services;
using Microsoft.EntityFrameworkCore;

namespace Homies.Tests
{
    [TestFixture]
    internal class EventServiceTests
    {
        private HomiesDbContext _dbContext;
        private EventService _eventService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<HomiesDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use unique database name to avoid conflicts
                .Options;
            _dbContext = new HomiesDbContext(options);

            _eventService = new EventService(_dbContext);
        }

        [Test]
        public async Task AddEventAsync_ShouldAddEvent_WhenValidEventModelAndUserId()
        {
            //Step 1: Arrange - Set up the initial conditions for the test
            //Create a new event model with test data
            var eventModel = new EventFormModel
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2)
            };

            //Define a user ID for testing purposes
            string userId = "testUserId";

            // Step 2: Act - Perform the action being tested
            //Call the service method to add the event

            await _eventService.AddEventAsync(eventModel, userId);

            //Step 3: Assert - Verify the outcome of the action
            //Retrieve the added event from the database

            var eventInTheDataBase = await _dbContext.Events
                .FirstOrDefaultAsync(x => x.Name == eventModel.Name && x.OrganiserId == userId);

            // Assert that the added event is not null, indicating it was successfully added
            Assert.IsNotNull(eventInTheDataBase);

            //Assert that the description of the added event matches the description provided in the event model
            Assert.That(eventInTheDataBase.Description, Is.EqualTo(eventModel.Description));
            Assert.That(eventInTheDataBase.Start, Is.EqualTo(eventModel.Start));
            Assert.That(eventInTheDataBase.End, Is.EqualTo(eventModel.End));
        }

        [Test]
        public async Task GetAllEventsAsync_ShouldReturnAllEvents()
        {
            // Step 1: Arrange - Set up the initial conditions for the test
            // Create two event models with test data
            var frirstEventModel = new EventFormModel
            {
                Name = "First Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2)
            };

            var seconfEventModel = new EventFormModel
            {
                Name = "Second Test Event",
                Description = "Test Description",
                Start = DateTime.Now.AddDays(2),
                End = DateTime.Now.AddDays(3)
            };

            // Define a user ID for testing purposes
            string userId = "testUserId";

            await _eventService.AddEventAsync(frirstEventModel, userId);
            await _eventService.AddEventAsync(seconfEventModel, userId);

            // Step 2: Act - Perform the action being tested
            // Add the two events to the database using the event service

            var result = await _eventService.GetAllEventsAsync();

            // Step 3: Act - Retrieve the count of events from the database
            var eventsCount = _dbContext.Events.Count();

            // Step 4: Assert - Verify the outcome of the action
            // Assert that the count of events in the database is equal to the expected count (2)
            Assert.That(eventsCount, Is.EqualTo(2));
            //TODO Add Asser for all prop

        }

        [Test]
        public async Task GetEventDetailAsync_ShoulsReturnAllEventDetails()
        {
            //Arrange
            var eventModel = new EventFormModel
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = 2,
            };

            await _eventService.AddEventAsync(eventModel, "nonExistingUserId");
            //Define a user ID for testing purposes
            //string userId = "testUserId";
            var eventInTheDb = await _dbContext.Events.FirstAsync();

            //Act
            var result = await _eventService.GetEventDetailsAsync(eventInTheDb.Id);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Name, Is.EqualTo(eventModel.Name));
            Assert.That(result.Description, Is.EqualTo(eventModel.Description));
            //Assert.That(result.Start, Is.EqualTo(eventModel.Start));
            //Assert.That(result.End, Is.EqualTo(eventModel.End));
            //Assert.That(result.TypeId, Is.EqualTo(eventModel.TypeId));
        }
        [Test]
        public async Task GetEventForEditAsync_ShouldGetEventIfPresentInTheDb()
        {
            //Arrange
            var eventModel = new EventFormModel
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = 2,
            };

            await _eventService.AddEventAsync(eventModel, "nonExistingUserId");
            //Define a user ID for testing purposes
            //string userId = "testUserId";
            var eventInTheDb = await _dbContext.Events.FirstAsync();

            //Act
            var result = await _eventService.GetEventForEditAsync(eventInTheDb.Id);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Name, Is.EqualTo(eventModel.Name));
            Assert.That(result.Description, Is.EqualTo(eventModel.Description));
            Assert.That(result.Start, Is.EqualTo(eventModel.Start));
            Assert.That(result.TypeId, Is.EqualTo(eventModel.TypeId));
        }

        [Test]
        public async Task GetEventForEditAsync_ShouldReturnNullIfEventIsNotFound()
        {
            //Act &Assert
            var result = await _eventService.GetEventForEditAsync(90);
            Assert.IsNull(result);
        }
        [Test]
        public async Task GetEventOrganizerIdAsync_ShouldReturnOrganaizerIdIfExist()
        {
            //Arrange
            var eventModel = new EventFormModel
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = 2,
            };
            const string userId = "userId";

            await _eventService.AddEventAsync(eventModel, userId);

            var eventInTheDb = await _dbContext.Events.FirstAsync();

            //Act
            var result = await _eventService.GetEventOrganizerIdAsync(eventInTheDb.Id);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo(userId));
        }

        [Test]
        public async Task GetEventOrganizerIdAsync_ShouldReturnNulIfEventIsNonExisting()
        {
            //Act
            var result = await _eventService.GetEventOrganizerIdAsync(77);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetUserJoinedEventsAsync_ShouldReturnAllJoinedUsers()
        {
            //Arrange
            const string userId = "userId";
            var testType = new Data.Models.Type
            {
                Name = "TestType",
            };
            await _dbContext.Types.AddAsync(testType);
            await _dbContext.SaveChangesAsync();

            var testEvent = new Event
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = testType.Id,
                OrganiserId = userId,
            };
            await _dbContext.Events.AddAsync(testEvent);
            await _dbContext.SaveChangesAsync();

            var testEventParticipant = new EventParticipant
            {
                EventId = testEvent.Id,
                HelperId = userId,
            };
            await _dbContext.EventsParticipants.AddAsync(testEventParticipant);
            await _dbContext.SaveChangesAsync();

            //Act
            var result = await _eventService.GetUserJoinedEventsAsync(userId);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(1));
            var eventParticipant = result.First();

            Assert.That(eventParticipant.Id, Is.EqualTo(testEvent.Id));
            Assert.That(eventParticipant.Name, Is.EqualTo(testEvent.Name));
        }

        [Test]
        public async Task JoinEventShouldReturnFalseIFEventDoesNotExist()
        {
            //Act
            var result = await _eventService.JoinEventAsync(99, "");

            //Assert
            Assert.False(result);

        }

        [Test]
        public async Task JoinEventShould_ReturnFalseIfWeAreParticipantInThisEvent()
        {
            //Arrange
            const string userId = "userId";

            //Create, Add and Save Event Type to Database
            var testType = new Data.Models.Type
            {
                Name = "TestType",
            };
            await _dbContext.Types.AddAsync(testType);
            await _dbContext.SaveChangesAsync();

            //Create,  Add and Save Event to Database
            var testEvent = new Event
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = testType.Id,
                OrganiserId = userId,
            };
            await _dbContext.Events.AddAsync(testEvent);
            await _dbContext.SaveChangesAsync();

            //Create, Add and save user that is also EventParticipant to Database
            var testEventParticipant = new EventParticipant
            {
                EventId = testEvent.Id,
                HelperId = userId,
            };
            await _dbContext.EventsParticipants.AddAsync(testEventParticipant);
            await _dbContext.SaveChangesAsync();

            //Act
            var result = await _eventService.JoinEventAsync(testEvent.Id, userId);

            //Assert
            Assert.False(result);

        }

        [Test]
        public async Task JoinEventShould_ReturnTrueIfTheUserIsSuccessfullyAddedToThisEvent()
        {
            //Arrange
            const string userId = "userId";

            //Create, Add and Save Event Type to Database
            var testType = new Data.Models.Type
            {
                Name = "TestType",
            };
            await _dbContext.Types.AddAsync(testType);
            await _dbContext.SaveChangesAsync();

            //Create,  Add and Save Event to Database
            var testEvent = new Event
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = testType.Id,
                OrganiserId = userId,
            };
            await _dbContext.Events.AddAsync(testEvent);
            await _dbContext.SaveChangesAsync();

            //Act
            var result = await _eventService.JoinEventAsync(testEvent.Id, userId);

            //Assert
            Assert.True(result);

        }

        [Test]
        public async Task LeaveEventShouldReturnFalse_IfUserIsNotJoinedInEvet()
        {
            //Assert
            const string userId = "not-signed-User";

            //Act
            var result = await _eventService.LeaveEventAsync(123, userId);

            //Assert
            Assert.False(result);

        }

        [Test]
        public async Task LeaveEventShouldReturnTrue_IfUserIsJoinedInEvet()
        {

            //Arrange
            //Create, Add and Save Event Type to Database
            var testType = new Data.Models.Type
            {
                Name = "TestType",
            };
            await _dbContext.Types.AddAsync(testType);
            await _dbContext.SaveChangesAsync();

            //Create,  Add and Save Event to Database
            var testEvent = new Event
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = testType.Id,
                OrganiserId = "a-sample-user",
            };
            await _dbContext.Events.AddAsync(testEvent);
            await _dbContext.SaveChangesAsync();

            string userId = "new-participant";
            await _eventService.JoinEventAsync(testEvent.Id, userId);

            //Create, Add and save user that is also EventParticipant to Database
            //Below is the same method for adding user as above method,  but is communicating directly with DataBase.
            //The method above is doing the same Task behind the logic
            //var testEventParticipant = new EventParticipant
            //{
            //    EventId = testEvent.Id,
            //    HelperId = userId,
            //};
            //await _dbContext.EventsParticipants.AddAsync(testEventParticipant);
            //await _dbContext.SaveChangesAsync();


            //Act
            var result = await _eventService.LeaveEventAsync(testEvent.Id, userId);

            //Asser
            Assert.True(result);

        }

        [Test]
        public async Task UpdateEventShouldReturnFalse_IfEventDesNotExist()
        {
            // Act
            var result = await _eventService.UpdateEventAsync(999, new EventFormModel { }, "user-Id");

            //Assert
            Assert.False(result);
        }

        [Test]
        public async Task UpdateEventShouldReturnFalse_IfOrganizerOfTheEventISDifferent()
        {
            //Arrange
            const string firstUserID = "firsttUserId";
            const string secondUserID = "secondUserId";

            //Create, Add and Save Event Type to Database
            var testType = new Data.Models.Type
            {
                Name = "TestType",
            };
            await _dbContext.Types.AddAsync(testType);
            await _dbContext.SaveChangesAsync();

            //Create,  Add and Save Event to Database
            var testEvent = new Event
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = testType.Id,
                OrganiserId = firstUserID,
            };
            await _dbContext.Events.AddAsync(testEvent);
            await _dbContext.SaveChangesAsync();
            //Act
            var result = await _eventService.UpdateEventAsync(testEvent.Id, new EventFormModel { }, secondUserID);

            //Assert
            Assert.False(result);
        }

        [Test]
        public async Task UpdateEventShouldReturnTrue_IfEventIsSuccessfulUpdated()
        {
            //Arrange
            const string firstUserID = "firstUserId";

            //Create, Add and Save Event Type to Database
            var testType = new Data.Models.Type
            {
                Name = "TestType",
            };
            await _dbContext.Types.AddAsync(testType);
            await _dbContext.SaveChangesAsync();

            //Create,  Add and Save Event to Database
            var testEvent = new Event
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = testType.Id,
                OrganiserId = firstUserID,
            };
            await _dbContext.Events.AddAsync(testEvent);
            await _dbContext.SaveChangesAsync();

            //Act
            var result = await _eventService.UpdateEventAsync(testEvent.Id, new EventFormModel {
                Name = "UpdatedName",
                Description = testEvent.Description,
                Start = testEvent.Start,
                End = testEvent.End,
            }, firstUserID);

            //Assert
            Assert.True(result);

            var eventFromDb = await _dbContext.Events.FirstOrDefaultAsync(x =>x.Id == testEvent.Id);
            Assert.NotNull(eventFromDb);
            Assert.That(eventFromDb.Name, Is.EqualTo("UpdatedName"));
        }
        [Test]
        public async Task GetAllTypeAsyncShouldReturnListOfEvents()
        {
            //Create, Add and Save Event Type to Database
            var testType = new Data.Models.Type
            {
                Name = "TestType",
            };
            await _dbContext.Types.AddAsync(testType);
            await _dbContext.SaveChangesAsync();

            //Act
            var result = await _eventService.GetAllTypesAsync();

            //Assert
            Assert.NotNull(result);
            Assert.That(result.Count(), Is.EqualTo(1));

            var singleType = result.First();
            Assert.That(singleType.Name, Is.EqualTo("TestType"));

        }

        [Test]
        public async Task IsUserJoinedEventAsyncShouldReturnFalseIfUserNotExist()
        {
            //Assert
            //Create, Add and Save Event Type to Database
            var testType = new Data.Models.Type
            {
                Name = "TestType",
            };
            await _dbContext.Types.AddAsync(testType);
            await _dbContext.SaveChangesAsync();

            //Create,  Add and Save Event to Database
            var testEvent = new Event
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = testType.Id,
                OrganiserId = "sample-User",
            };
            await _dbContext.Events.AddAsync(testEvent);
            await _dbContext.SaveChangesAsync();
            //Act
            var result = await _eventService.IsUserJoinedEventAsync(testEvent.Id, "notExistingUserId");

            //Assert
            Assert.False(result);
        }

        [Test]
        public async Task IsUserJoinedEventAsyncShouldReturnFalseIfEventDoesNotExist()
        {
            //Act
            var result = await _eventService.IsUserJoinedEventAsync(999, "currentUserId");

            //Assert
            Assert.False(result);
        }

        [Test]
        public async Task IsUserJoinedEventAsyncShouldReturnTrueIfJoined()
        {
            //Assert
            //Create, Add and Save Event Type to Database
            var testType = new Data.Models.Type
            {
                Name = "TestType",
            };
            await _dbContext.Types.AddAsync(testType);
            await _dbContext.SaveChangesAsync();

            //Create,  Add and Save Event to Database
            var testEvent = new Event
            {
                Name = "Test Event",
                Description = "Test Description",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(2),
                TypeId = testType.Id,
                OrganiserId = "sample-User",
            };
            await _dbContext.Events.AddAsync(testEvent);
            await _dbContext.SaveChangesAsync();

            await _eventService.JoinEventAsync(testEvent.Id, "joinedUserId");
            //Act
            var result = await _eventService.IsUserJoinedEventAsync(testEvent.Id, "joinedUserId");

            //Assert
            Assert.True(result);
        }
    }
}
