using AutomatedWebAPITests.Models;
using AutomatedWebAPITests.TestHelpers;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth2;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

namespace AutomatedWebAPITests.Tests
{
    public class APITests
    {
        private RestClient client;
        private RestClientOptions options;
        public const string baseURL = "https://demoqa.com";
        public string bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyTmFtZSI6IlZhbGlkVXNlck5hbWUxMTExIiwicGFzc3dvcmQiOiJQYXNzQDEyMzMiLCJpYXQiOjE2OTA3MDY4MDN9.thg5vt2HZt5r3qPzbposB-2o7Fw2KQ_jk23e2jiEeGY";


        [SetUp]
        public void Setup()
        {
            this.options = new RestClientOptions(baseURL);
            options.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            this.client = new RestClient(options);
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        [OneTimeTearDown]
        public void EndTest()
        {
            Trace.Flush();
        }

        [TestCase("ValidUserName4", "Pass@123", System.Net.HttpStatusCode.Created)]
        [TestCase("ValidUserName5", "Pass@1234", System.Net.HttpStatusCode.Created)]
        [TestCase("ValidUserName6", "Pass@12345", System.Net.HttpStatusCode.Created)]
        [Category("Task 2")]
        [Test]
        public void Test_SuccessfulyCreatingNewUser(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {
            //Preconditions
            Trace.WriteLine($"Preconditions: the user shouldn't be previously created in the DB\nPasswords must have at least one non alphanumeric character, one digit ('0'-'9'), one uppercase ('A'-'Z'), one lowercase ('a'-'z'), one special character and Password must be eight characters or longer");

            var request = new RestRequest("/account/v1/user", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            //Test case information
            Trace.WriteLine($"Test case: Creating new user with userName: {userName}, password: {password}");

            var newUser = new
            {
                userName = userName,
                password = password
            };

            request.AddBody(newUser);

            Trace.WriteLine("Sending the request to create a new user");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            //Assertions
            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var user = System.Text.Json.JsonSerializer.Deserialize<Users>(response.Content);

            Assert.That(user.userID, Is.Not.Empty);
            Assert.That(user.userID, Is.InstanceOf<string>());
            Trace.WriteLine($"Validated userID: {user.userID}");
            Assert.That(user.username, Is.Not.Empty);
            Assert.That(user.username, Is.EqualTo(userName));
            Assert.That(user.username, Is.InstanceOf<string>());
            Trace.WriteLine($"Validated username: {user.username}");
            Assert.That(user.books, Is.InstanceOf<List<Book>>());
            Assert.That(user.books.Count, Is.EqualTo(0));
            Trace.WriteLine($"Validated user books: {user.books.Count}");

        }

        [TestCase("ValidUserName1111", "Pass@1233", System.Net.HttpStatusCode.OK)]
        public void Test_GeneratingToken(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {

            Trace.WriteLine($"Preconditions: the user should be previously created in the DB");
            var tokenRequest = new RestRequest("/account/v1/generateToken", Method.Post);

            Trace.WriteLine($"Test case: Generating a token for userName: {userName}, password: {password}");

            var verifiedUser = new
            {
                userName = userName,
                password = password
            };

            tokenRequest.AddJsonBody(verifiedUser);
            Trace.WriteLine("Sending the request to create a new token");
            var responseToken = client.Execute(tokenRequest);
            var token = System.Text.Json.JsonSerializer.Deserialize<Token>(responseToken.Content);

            Trace.WriteLine($"Getting the token for userName: {userName}, password: {password}");

            var apiToken = token.token;

            Trace.WriteLine($"The token is: {apiToken}");

            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, responseToken.StatusCode);


        }

        [TestCase("ValidUserName1111", "Pass@1233", System.Net.HttpStatusCode.OK)]
        public void Test_AuthorizationCheck(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {
            Trace.WriteLine($"Preconditions: the user should be previously created in the DB\n the user should be with already generated valid token");
            var authorizedRequest = new RestRequest("/account/v1/authorized", Method.Post);

            Trace.WriteLine($"Test case: Checking if the user is authorized - userName: {userName}, password: {password}");

            var authorizedUser = new
            {
                userName = userName,
                password = password
            };

            authorizedRequest.AddJsonBody(authorizedUser);
            Trace.WriteLine("Sending the request to check if the user is authorized");
            var responseAuthorized = client.Execute(authorizedRequest);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            var isAuthorized = JsonConvert.DeserializeObject<bool>(responseAuthorized.Content);

            Trace.WriteLine($"Validated authorization response: {isAuthorized}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, responseAuthorized.StatusCode);
            Assert.IsTrue(isAuthorized);

        }

        //verifying the 404 messages
        [Category("Task 1")]
        [TestCase("validusername", "stringggg", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "stringgg", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "stringg1", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "stringg@", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "stringgG", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "stringG1", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "stri@G1", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "stringg", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "stringG", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "strin1G", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "11111111", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "@@@@@@@@", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", "AAAAAAAA", System.Net.HttpStatusCode.BadRequest)]
        [Test]
        public void Test_VerifyStatusCode_CreatingNewUser_ReturnsBadRequest(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {

            var request = new RestRequest("/account/v1/user", Method.Post);

            Trace.WriteLine($"Test case: Veryfing the 404 message with unacceptable password: {password}");

            var newUser = new
            {
                userName = userName,
                password = password
            };
            request.AddJsonBody(newUser);

            Trace.WriteLine("Sending the request to create a new user with wrong password");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Trace.WriteLine($"Validate the code response response");
            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.code, Is.EqualTo("1300"));
            Assert.That(issue.code, Is.InstanceOf<int>(), "The value should be returned as an integer data type");
            Trace.WriteLine($"Validate the message response response");
            Assert.That(issue.message, Is.InstanceOf<string>());
            Assert.That(issue.message, Is.Not.Empty);
            Assert.That(issue.message, Is.EqualTo("Passwords must have at least one non alphanumeric character, one digit ('0'-'9'), one uppercase ('A'-'Z'), one lowercase ('a'-'z'), one special character and Password must be eight characters or longer."));
        }

        //verifying the 404 messages
        [Category("Task 1")]
        [TestCase("validusername", "", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("", "", System.Net.HttpStatusCode.BadRequest)]
        [TestCase(null, null, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(null, "ValidPassword123!", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", null, System.Net.HttpStatusCode.BadRequest)]
        [TestCase("", "ValidPassword123!", System.Net.HttpStatusCode.BadRequest)]
        [Test]
        public void Test_VerifyStatusCode_CreatingNewUserWithEmptyField_ReturnsBadRequest(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {
            var request = new RestRequest("/account/v1/user", Method.Post);

            Trace.WriteLine($"Test case: Verifying the 404 message with empty userName: {userName}, or password: {password}");

            request.AddHeader("Content-Type", "application/json");

            var newUser = new
            {
                userName = userName,
                password = password
            };
            request.AddJsonBody(newUser);

            Trace.WriteLine("Sending the request to create a new user with empty username or password");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Trace.WriteLine($"Validate the code response response");
            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.code, Is.EqualTo("1200"));
            Assert.That(issue.code, Is.InstanceOf<int>(), "The value should be returned as an integer data type");
            Trace.WriteLine($"Validate the message response response");
            Assert.That(issue.message, Is.InstanceOf<string>());
            Assert.That(issue.message, Is.Not.Empty);
            Assert.That(issue.message, Is.EqualTo("UserName and Password required."));
        }

        //This test should verify the 406 error
        //I changed the Content-Type value to be xml and it should've return "NotAcceptable"
        [TestCase("ValidUserName1111", "Pass@1233", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validUserName", "1Aa@1Aa##$", System.Net.HttpStatusCode.BadRequest)]
        [Category("Task 1")]
        [Test]
        public void Test_VerifyStatusCode_CreatingNewUser_ReturnsNotAcceptable(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {

            var request = new RestRequest("/account/v1/user", Method.Post);

            Trace.WriteLine($"Test case: Verifying the 406 message adding xml as a content type");

            request.AddHeader("Content-Type", "application/xml");

            var newUser = new
            {
                userName = userName,
                password = password
            };

            request.AddJsonBody(newUser);

            Trace.WriteLine("Sending the request to create a new user");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Trace.WriteLine($"Validate the code response response");
            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.code, Is.InstanceOf<int>(), "The value should be returned as an integer data type");
            Trace.WriteLine($"Validate the message response response");
            Assert.That(issue.message, Is.Not.Empty);
            Assert.That(issue.message, Is.InstanceOf<string>());
        }

        //This test should verify the 406 error
        [TestCase("ValidUserName1111", "Pass@1233", System.Net.HttpStatusCode.NotAcceptable)]
        [Category("Task 1")]
        [Test]
        public void Test_VerifyStatusCode_UsingAlreadyRegisteredUser_ReturnsNotAcceptable(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {
            Trace.WriteLine($"Preconditions: the {userName} should be previously created in the DB");
            var request = new RestRequest("/account/v1/user", Method.Post);

            Trace.WriteLine($"Test case: Verifying the 406 message with userName: {userName}, password: {password}");

            request.AddHeader("Content-Type", "application/json");

            var newUser = new
            {
                userName = userName,
                password = password
            };

            request.AddJsonBody(newUser);

            Trace.WriteLine("Sending the request to create an already created user");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Trace.WriteLine($"Validate the code response response");
            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.code, Is.EqualTo("1204"));
            Assert.That(issue.code, Is.InstanceOf<int>(), "The value should be returned as an integer data type");
            Trace.WriteLine($"Validate the message response response");
            Assert.That(issue.message, Is.Not.Empty);
            Assert.That(issue.message, Is.EqualTo("User exists!"));
            Assert.That(issue.message, Is.InstanceOf<string>());

        }

        //[TestCase(9.99, 1.11, System.Net.HttpStatusCode.BadRequest)]
        //[TestCase(9.9999999, 1.11, System.Net.HttpStatusCode.BadRequest)]
        //[TestCase(9.999999, 1.11, System.Net.HttpStatusCode.BadRequest)]
        //[TestCase(9.99, 1.1111111, System.Net.HttpStatusCode.BadRequest)]
        //[TestCase(9.99, 1.111111, System.Net.HttpStatusCode.BadRequest)]
        //[TestCase(0, 0, System.Net.HttpStatusCode.BadRequest)]
        //[Category("Task 1")]
        //[Test]
        //public void CreateNewUser_WithDoubleDataType_BadRequest(double userName, double password, System.Net.HttpStatusCode expectedStatusCode)
        //{
        //    var request = new RestRequest("/account/v1/user", Method.Post);

        //    Trace.WriteLine($"Test case: Verifying the 404 message adding double in userName: {userName}, password: {password}");

        //    request.AddHeader("Content-Type", "application/json");

        //    var newUser = new
        //    {
        //        userName = userName,
        //        password = password
        //    };
        //    request.AddJsonBody(newUser);

        //    Trace.WriteLine("Sending the request to create a new user with double data type");
        //    var response = client.Execute(request);
        //    Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

        //    Assert.AreEqual(expectedStatusCode, response.StatusCode);
        //}

        //[TestCase(99999999, 111, System.Net.HttpStatusCode.BadRequest)]
        //[TestCase(999, 11111111, System.Net.HttpStatusCode.BadRequest)]
        //[TestCase(999999999, 111, System.Net.HttpStatusCode.BadRequest)]
        //[TestCase(999, 111111111, System.Net.HttpStatusCode.BadRequest)]
        //[TestCase(0, 0, System.Net.HttpStatusCode.BadRequest)]
        //[Category("Task 1")]
        //[Test]
        //public void CreateNewUser_WithIntDataType_BadRequest(int userName, int password, System.Net.HttpStatusCode expectedStatusCode)
        //{
        //    var request = new RestRequest("/account/v1/user", Method.Post);

        //    Trace.WriteLine($"Test case: Verifying the 404 message adding integer in userName: {userName}, password: {password}");

        //    request.AddHeader("Content-Type", "application/json");

        //    var newUser = new
        //    {
        //        userName = userName,
        //        password = password
        //    };
        //    request.AddJsonBody(newUser);

        //    Trace.WriteLine("Sending the request to create a new user with int data type");
        //    var response = client.Execute(request);
        //    Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

        //    Assert.AreEqual(expectedStatusCode, response.StatusCode);
        //}

        [TestCase('a', 'b', System.Net.HttpStatusCode.BadRequest)]
        [TestCase('A', 'B', System.Net.HttpStatusCode.BadRequest)]
        [TestCase('~', '@', System.Net.HttpStatusCode.BadRequest)]
        [TestCase('1', '2', System.Net.HttpStatusCode.BadRequest)]
        [Category("Task 1")]
        public void Test_CreateUser_WithCharDataType_BadRequest(char userName, char password, System.Net.HttpStatusCode expectedStatusCode)
        {
            var request = new RestRequest("/account/v1/user", Method.Post);

            Trace.WriteLine($"Test case: Verifying the 404 message adding char in userName: {userName}, password: {password}");

            request.AddHeader("Content-Type", "application/json");

            var newUser = new
            {
                userName = userName,
                password = password
            };
            request.AddJsonBody(newUser);
            Trace.WriteLine("Sending the request to create a new user with char data type");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            Assert.AreEqual(expectedStatusCode, response.StatusCode);


        }

        [Test]
        [TestCase("ValidUserName1111", "Pass@1233")]
        [Category("Task 3")]
        public void Test_AddingABook_ToList(string userName, string password)
        {
            Trace.WriteLine($"Preconditions: the {userName} should be previously authorized");

            var requestBooks = new RestRequest("/bookstore/v1/books", Method.Post);

            Trace.WriteLine($"Test case: Adding a book to the favorites list of userName: {userName}, password: {password}");

            requestBooks.AddHeader("Content-Type", "application/json");

            var userId = "14fb57d0-85a2-4d96-820f-d278330da6a0";

            var addListOfBooks = new
            {
                userId = userId,
                collectionOfIsbns = new List<Book>{
                     new Book{ isbn = "9781449325862" },
                     new Book{ isbn = "9781449331818" },
                     new Book{ isbn = "9781449337711" }
                }
            };

            requestBooks.AddBody(addListOfBooks);

            Trace.WriteLine("Sending the request to add the books to the list");
            var response = client.Execute(requestBooks);
            Trace.WriteLine($"Expected response with status code: {System.Net.HttpStatusCode.Created}");

            var bookWrapper = JsonConvert.DeserializeObject<BookWrapper>(response.Content);

            List<Book> allBooks = bookWrapper.books;

            var addedCollectionOfIsbns = addListOfBooks.collectionOfIsbns;

            Assert.IsNotNull(bookWrapper);
            Trace.WriteLine($"Validate if the amount of isbn is more than 0");
            Assert.IsTrue(allBooks.Count() > 0);
            for (int i = 0; i < allBooks.Count; i++)
            {
                Trace.WriteLine($"Validate if the response isbn's are not null");
                Assert.IsNotNull(allBooks[i].isbn);
                Trace.WriteLine($"Validate if the returned isbns from the server are equal to the {addedCollectionOfIsbns[i].isbn}");
                Assert.AreEqual(addedCollectionOfIsbns[i].isbn, allBooks[i].isbn);

            }


        }

        [Test]
        [Category("Task 4")]
        public void Test_AddingNonExistentBook_ToList()
        {
            Trace.WriteLine($"Preconditions: the user should be previously authorized");

            var request = new RestRequest("/bookstore/v1/books", Method.Post);

            Trace.WriteLine($"Test case: Adding a non-existent book to the favorites list");

            request.AddHeader("Content-Type", "application/json");

            var userId = "14fb57d0-85a2-4d96-820f-d278330da6a0";

            var addListOfBooks = new
            {
                userId = userId,
                collectionOfIsbns = new List<Book>{
                     new Book{ isbn = "9781449862" } //not a real number
                }

            };

            request.AddBody(addListOfBooks);
            Trace.WriteLine($"Sending the request to add the non-existent book with 9781449862 to the list");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {System.Net.HttpStatusCode.BadRequest}");

            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Trace.WriteLine($"Validate the code response response");
            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.code, Is.InstanceOf<int>(), "The value should be returned as an integer data type");

            Trace.WriteLine($"Validate the message response response");
            Assert.That(issue.message, Is.Not.Empty);
            Assert.That(issue.message, Is.InstanceOf<string>());


        }


        [Test]
        [Category("Task 5")]
        [TestCase("14fb57d0-85a2-4d96-820f-d278330da6a0", "9781449325862", "9781449337711", System.Net.HttpStatusCode.OK)]
        public void Test_ReplacingTheFirstBook_InBooksList(string userId, string firstBookIsbn, string lastBookIsbn, System.Net.HttpStatusCode expectedStatusCode)
        {

            Trace.WriteLine($"Preconditions: the user should be previously authorized");

            var request = new RestRequest($"/bookstore/v1/books/{firstBookIsbn}", Method.Put);

            request.AddHeader("Content-Type", "application/json");

            var lastBookInList = new
            {
                userId = userId,
                isbn = lastBookIsbn
            };

            request.AddBody(lastBookInList);

            Trace.WriteLine("Sending the request to swap the first and the last book in the list");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            // adding the first book to the last place
            var requestAddingFirstBook = new RestRequest("/bookstore/v1/books", Method.Post);

            var addListOfBooks = new
            {
                userId = userId,
                collectionOfIsbns = new List<Book>{
                     new Book{ isbn = "9781449325862" } // first book
                }

            };

            requestAddingFirstBook.AddBody(addListOfBooks);

            Trace.WriteLine("Sending the request to add a book to the list");
            var responseAddingFirstBook = client.Execute(requestAddingFirstBook);
            Trace.WriteLine($"Expected response with status code: {System.Net.HttpStatusCode.Created}");


            var bookWrapper = JsonConvert.DeserializeObject<BookWrapper>(responseAddingFirstBook.Content);

            List<Book> allBooks = bookWrapper.books;
            var firstBook = allBooks[0];
            var lastBook = allBooks[allBooks.Count];

            Assert.That(allBooks[0], Is.Not.Empty);
            Assert.That(firstBook.isbn, Is.EqualTo(lastBookInList.isbn));
            Assert.That(allBooks[allBooks.Count].isbn, Is.EqualTo(lastBookInList.isbn));


        }

        [Test]
        [Category("Task 6")]
        [TestCase("9781491904244", System.Net.HttpStatusCode.OK)]
        public void Test_ValidateTheNumberOfPagesOfABook(string isbn, System.Net.HttpStatusCode expectedStatusCode)
        {
            Trace.WriteLine($"Preconditions: the user should be previously authorized");

            var request = new RestRequest($"BookStore/v1/Book?ISBN={isbn}", Method.Get);

            Trace.WriteLine($"Test case: Validating the number of pages of a book");

            request.AddHeader("Content-Type", "application/json");

            Trace.WriteLine($"Sending a get request to receive the keys-values of the book with {isbn}");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var book = System.Text.Json.JsonSerializer.Deserialize<Book>(response.Content);

            int numberOfPages = book.pages;

            int expectedPages = 278;

            Assert.IsNotNull(book.pages);
            Assert.That(book.pages, Is.InstanceOf<int>());
            Assert.AreEqual(expectedPages, numberOfPages);

        }

        [Test]
        [Category("Task 7")]
        [TestCase("14fb57d0-85a2-4d96-820f-d278330da6a0", "9781449337711", System.Net.HttpStatusCode.NoContent)]
        public void Test_RemovingABook_FromTheList(string userId, string isbn, System.Net.HttpStatusCode expectedStatusCode)
        {
            Trace.WriteLine($"Preconditions: the user should be previously authorized");

            var request = new RestRequest("BookStore/v1/Book", Method.Delete);

            Trace.WriteLine($"Test case: Removing a certain book from the books list");

            request.AddHeader("Content-Type", "application/json");

            var deletedBook = new Book
            {

                isbn = isbn,
                userId = userId
            };

            request.AddBody(deletedBook);

            Trace.WriteLine($"Sending a request to remove the book with isbn: {isbn}");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            if(response.StatusCode == expectedStatusCode)
            {
                var message = System.Text.Json.JsonSerializer.Deserialize<SuccessfulyDeletedResponse>(response.Content);

                Assert.That(message.message, Is.Not.Empty);
                Assert.That(message.isbn, Is.Not.Empty);
                Assert.That(message.userId, Is.Not.Empty);
                Assert.That(message.isbn, Is.EqualTo(deletedBook.isbn));
                Assert.That(message.userId, Is.EqualTo(deletedBook.userId));

                var requestListOfBooks = new RestRequest("BookStore/v1/Books", Method.Get);

                var responseListOfBooks = client.Execute(requestListOfBooks);

                var bookWrapper = JsonConvert.DeserializeObject<BookWrapper>(responseListOfBooks.Content);

                List<Book> allBooks = bookWrapper.books;

                for (int i = 0; i < allBooks.Count(); i++)
                {
                    Assert.That(allBooks[i].isbn, Is.Not.EqualTo(deletedBook.isbn));
                    Assert.IsNotEmpty(allBooks[i].isbn);
                }
            }

            else if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized) 
            {
                var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

                Assert.IsNotNull(issue);
                Trace.WriteLine($"Validate the code response");
                Assert.That(issue.code, Is.Not.Empty);
                Assert.That(issue.code, Is.EqualTo("1200"));
                Assert.That(issue.code, Is.InstanceOf<int>(), "The value should be returned as an integer data type");
                Trace.WriteLine($"Validate the message response");
                Assert.That(issue.code, Is.EqualTo("User not authorized!"));
                Assert.That(issue.message, Is.Not.Empty);
                Assert.That(issue.message, Is.InstanceOf<string>());
            }

        }

        [Test]
        [Category("Task 8")]
        [TestCase("14fb57d0-85a2-4d96-820f-d278330da6a0", "9781593275846555151", System.Net.HttpStatusCode.BadRequest)]
        public void Test_RemovingANonExistentBook_FromTheList(string userId, string incorrectIsbn, System.Net.HttpStatusCode expectedStatusCode)
        {

            Trace.WriteLine($"Preconditions: the user should be previously authorized");

            var request = new RestRequest("BookStore/v1/Book", Method.Delete);

            Trace.WriteLine($"Test case: Removing a book with incorrect isbn : {incorrectIsbn}");

            request.AddHeader("Content-Type", "application/json");

            var deleteBook = new
            {
                userId = userId,
                isbn = incorrectIsbn
            };

            request.AddBody(deleteBook);

            Trace.WriteLine($"Sending a request to remove the book with incorrect isbn: {incorrectIsbn}");
            var response = client.Execute(request);
            Trace.WriteLine($"Expected response with status code: {expectedStatusCode}");

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Assert.IsNotNull(issue);
            Trace.WriteLine($"Validate the code response");
            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.code, Is.InstanceOf<int>(), "The value should be returned as an integer data type");
            Trace.WriteLine($"Validate the message response");
            Assert.That(issue.message, Is.Not.Empty);
            Assert.That(issue.message, Is.InstanceOf<string>());


        }


    }

}
