using AutomatedWebAPITests.Models;
using AutomatedWebAPITests.TestHelpers;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth2;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Xml.Linq;

namespace AutomatedWebAPITests.Tests
{
    public class APITests
    {
        private RestClient client;
        private RestClientOptions options;
        public const string baseURL = "https://demoqa.com";
        //public string bearerToken = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyTmFtZSI6InZhbGlkVXNlck5hbWUiLCJwYXNzd29yZCI6IjFBYUAxQWEoIiwiaWF0IjoxNjkwMjIyODQ4fQ.oh5IEF5OzHuC8jj6Arsm--1hw6B5mRSDlTOhVR7DD7g";

        [SetUp]
        public void Setup()
        {
            this.options = new RestClientOptions(baseURL);
            //options.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken);
            this.client = new RestClient(options);
        }

        //verifying the 404 messages
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
        [Category("Task 1")]
        [Test]
        public void Test_VerifyStatusCode_CreatingNewUser_ReturnsBadRequest(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {
            var request = new RestRequest("/account/v1/user", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer tomislavilievqa"); //adding an extra header since I had issues with the authorization

            var newUser = new
            {
                userName = userName,
                password = password
            };
            request.AddJsonBody(newUser);

            var response = client.Execute(request);

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.message, Is.Not.Empty);
            Assert.That(issue.message, Is.EqualTo("Passwords must have at least one non alphanumeric character, one digit ('0'-'9'), one uppercase ('A'-'Z'), one lowercase ('a'-'z'), one special character and Password must be eight characters or longer."));
            Assert.That(issue.code, Is.EqualTo("1300"));
            //BUG -> based on specification the code message should be returned as an "int" data type
            Assert.That(issue.code, Is.InstanceOf<int>());
            Assert.That(issue.message, Is.InstanceOf<string>());
        }

        //verifying the 404 messages
        [TestCase("validusername", "", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("", "", System.Net.HttpStatusCode.BadRequest)]
        [TestCase(null, null, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(null, "ValidPassword123!", System.Net.HttpStatusCode.BadRequest)]
        [TestCase("validusername", null, System.Net.HttpStatusCode.BadRequest)]
        [TestCase("", "ValidPassword123!", System.Net.HttpStatusCode.BadRequest)]
        [Category("Task 1")]
        [Test]
        public void Test_VerifyStatusCode_CreatingNewUserWithEmptyField_ReturnsBadRequest(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {
            var request = new RestRequest("/account/v1/user", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer tomislavilievqa"); //adding an extra header since I had issues with the authorization

            var newUser = new
            {
                userName = userName,
                password = password
            };
            request.AddJsonBody(newUser);

            var response = client.Execute(request);

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.message, Is.Not.Empty);
            Assert.That(issue.message, Is.EqualTo("UserName and Password required."));
            Assert.That(issue.code, Is.EqualTo("1200"));
            //based on specification the code message should be returned as an "int" data type, that's a bug
            Assert.That(issue.code, Is.InstanceOf<int>());
            Assert.That(issue.message, Is.InstanceOf<string>());
        }

        //This test should verify the 406 error
        //I changed the Content-Type value to be xml and it should've return "NotAcceptable"
        [TestCase("validUserName", "1Aa@1Aa#", System.Net.HttpStatusCode.NotAcceptable)]
        [TestCase("validUserName", "1Aa@1Aa#$", System.Net.HttpStatusCode.NotAcceptable)]
        [Category("Task 1")]
        [Test]
        public void Test_VerifyStatusCode_CreatingNewUser_ReturnsNotAcceptable(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {

            var request = new RestRequest("/account/v1/user", Method.Post);

            request.AddHeader("Content-Type", "application/xml");

            var newUser = new
            {
                userName = userName,
                password = password
            };

            request.AddJsonBody(newUser);

            var response = client.Execute(request);

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.message, Is.Not.Empty);
            //based on specification the "code" value should be returned as an "int" data type, that's a bug
            Assert.That(issue.code, Is.InstanceOf<int>());
            Assert.That(issue.message, Is.InstanceOf<string>());
        }

        //This test should verify the 406 error
        //Preconditions: You need first to create a valid user
        [TestCase("validUserName", "1Aa@1Aa#", System.Net.HttpStatusCode.NotAcceptable)]
        [Category("Task 1")]
        [Test]
        public void Test_VerifyStatusCode_UsingAlreadyRegisteredUser_ReturnsNotAcceptable(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {

            var request = new RestRequest("/account/v1/user", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer tomislavilievqa"); //adding an extra header since I had issues with the authorization

            var newUser = new
            {
                userName = userName,
                password = password
            };

            request.AddJsonBody(newUser);

            var response = client.Execute(request);

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.message, Is.Not.Empty);
            Assert.That(issue.message, Is.EqualTo("User exists!"));
            Assert.That(issue.code, Is.EqualTo("1204"));
            //based on specification the "code" value should be returned as an "int" data type, that's a bug
            Assert.That(issue.code, Is.InstanceOf<int>());
            Assert.That(issue.message, Is.InstanceOf<string>());

        }

        //preconditions - the user shouldn't be previously created
        [TestCase("validUserName", "1Aa@1Aa#(", System.Net.HttpStatusCode.Created)]
        [TestCase("validUserName2", "1Aa@1Aa@#(", System.Net.HttpStatusCode.Created)]
        [TestCase("validUserName3", "1Aa@1Aa@#((", System.Net.HttpStatusCode.Created)]
        [Category("Task 2")]
        [Test]
        public void Test_VerifyStatusCode_SuccessfulyCreatingNewUser(string userName, string password, System.Net.HttpStatusCode expectedStatusCode)
        {

            // adding new valid user
            var request = new RestRequest("/account/v1/user", Method.Post);

            request.AddHeader("Content-Type", "application/json");

            var newUser = new
            {
                userName = userName,
                password = password
            };

            request.AddBody(newUser);

            var response = client.Execute(request);

            Assert.AreEqual(expectedStatusCode, response.StatusCode);

            var user = System.Text.Json.JsonSerializer.Deserialize<Users>(response.Content);

            Assert.That(user.userID, Is.Not.Empty);
            Assert.That(user.username, Is.Not.Empty);
            Assert.That(user.username, Is.EqualTo(userName));
            Assert.That(user.userID, Is.InstanceOf<string>());
            Assert.That(user.username, Is.InstanceOf<string>());
            Assert.That(user.books, Is.InstanceOf<List<Books>>());


            //var books = user.books;
            //var len = books.Count();

            //Assert.That(books[0], Is.Not.Empty);
            //Assert.That(books[0].isbn, Is.InstanceOf<string>());
            //Assert.That(books[0].title, Is.InstanceOf<string>());
            //Assert.That(books[0].subTitle, Is.InstanceOf<string>());
            //Assert.That(books[0].author, Is.InstanceOf<string>());
            //Assert.That(books[0].publish_date, Is.InstanceOf<DateFormat>());
            //Assert.That(books[0].publisher, Is.InstanceOf<string>());
            //Assert.That(books[0].pages, Is.InstanceOf<int>());
            //Assert.That(books[0].description, Is.InstanceOf<string>());
            //Assert.That(books[0].website, Is.InstanceOf<string>());
            //Assert.That(len, Is.EqualTo(9));

            //generating token >>>

            var tokenRequest = new RestRequest("/account/v1/generateToken", Method.Post);

            var verifiedUser = new
            {
                userName = userName,
                password = password
            };

            tokenRequest.AddJsonBody(verifiedUser);

            var responseToken = client.Execute(tokenRequest); 

            Assert.AreEqual(System.Net.HttpStatusCode.OK, responseToken.StatusCode);

            var token = System.Text.Json.JsonSerializer.Deserialize<Token>(responseToken.Content);

            var apiKey = token.token;

            // authorize check >>

            var authorizedRequest = new RestRequest("/account/v1/authorized", Method.Post);

            var authorizedUser = new
            {
                userName = userName,
                password = password
            };

            authorizedRequest.AddJsonBody(authorizedUser);

            var responseAuthorized = client.Execute(authorizedRequest);

            var isAuthorized = JsonConvert.DeserializeObject<bool>(responseAuthorized.Content);

            Assert.IsTrue(isAuthorized);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, responseAuthorized.StatusCode);

        }

        [TestCase(9.99, 1.11, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(9.9999999, 1.11, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(9.999999, 1.11, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(9.99, 1.1111111, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(9.99, 1.111111, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(0, 0, System.Net.HttpStatusCode.BadRequest)]
        [Category("Task 1")]
        [Test]
        public void CreateNewUser_WithDoubleDataType_BadRequest(double userName, double password, System.Net.HttpStatusCode expectedStatusCode)
        {
            var request = new RestRequest("/account/v1/user", Method.Post);

            request.AddHeader("Content-Type", "application/json");

            var newUser = new
            {
                userName = userName,
                password = password
            };
            request.AddJsonBody(newUser);

            var response = client.Execute(request);

            Assert.AreEqual(expectedStatusCode, response.StatusCode);
        }

        [TestCase(99999999, 111, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(999, 11111111, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(999999999, 111, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(999, 111111111, System.Net.HttpStatusCode.BadRequest)]
        [TestCase(0, 0, System.Net.HttpStatusCode.BadRequest)]
        [Category("Task 1")]
        [Test]
        public void CreateNewUser_WithIntDataType_BadRequest(int userName, int password, System.Net.HttpStatusCode expectedStatusCode)
        {
            var request = new RestRequest("/account/v1/user", Method.Post);

            request.AddHeader("Content-Type", "application/json");

            var newUser = new
            {
                userName = userName,
                password = password
            };
            request.AddJsonBody(newUser);

            var response = client.Execute(request);

            Assert.AreEqual(expectedStatusCode, response.StatusCode);
        }

        [TestCase('a', 'b', System.Net.HttpStatusCode.BadRequest)]
        [TestCase('A', 'B', System.Net.HttpStatusCode.BadRequest)]
        [TestCase('~', '@', System.Net.HttpStatusCode.BadRequest)]
        [TestCase('1', '2', System.Net.HttpStatusCode.BadRequest)]
        [Category("Task 1")]
        public void Test_CreateUser_WithCharDataType_BadRequest(char userName, char password, System.Net.HttpStatusCode expectedStatusCode)
        {
            var request = new RestRequest("/account/v1/user", Method.Post);

            request.AddHeader("Content-Type", "application/json");

            var newUser = new
            {
                userName = userName,
                password = password
            };
            request.AddJsonBody(newUser);

            var response = client.Execute(request);

            Assert.AreEqual(expectedStatusCode, response.StatusCode);


        }

        [Test]
        [TestCase("validUserName", "1Aa@1Aa(")]
        [Category("Task 3")]
        public void Test_AddingABook_ToList(string userName, string password)
        {
            // authorize check

            var authorizedRequest = new RestRequest("/account/v1/authorized", Method.Post);

            var authorizedUser = new
            {
                userName = userName,
                password = password
            };

            authorizedRequest.AddJsonBody(authorizedUser);

            var responseAuthorized = client.Execute(authorizedRequest);

            var isAuthorized = JsonConvert.DeserializeObject<bool>(responseAuthorized.Content);

            Assert.IsTrue(isAuthorized);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, responseAuthorized.StatusCode);

            // adding new book to the collection

            var requestBooks = new RestRequest("/bookstore/v1/books", Method.Post);

            requestBooks.AddHeader("Content-Type", "application/json");
            requestBooks.AddHeader("Authorization", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyTmFtZSI6InZhbGlkVXNlck5hbWUiLCJwYXNzd29yZCI6IjFBYUAxQWEoIiwiaWF0IjoxNjkwMjIyODQ4fQ.oh5IEF5OzHuC8jj6Arsm--1hw6B5mRSDlTOhVR7DD7g");

            var userId = "2ed75802-de8a-4652-9a2f-cfd00c0a3bcb";

            var addListOfBooks = new
            {
                userId = userId,
                collectionOfIsbns = new List<Books>{
                     new Books{ isbn = "9781449325862" }
                }
            };

            requestBooks.AddBody(addListOfBooks);

            var response = client.Execute(requestBooks);

            var books = System.Text.Json.JsonSerializer.Deserialize<Books>(response.Content);

            var addedCollectionOfIsbns = addListOfBooks.collectionOfIsbns;

            Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
            Assert.That(books.isbn, Is.Not.Empty);
            Assert.AreEqual(addedCollectionOfIsbns[0].isbn, books.isbn);

        }

        //4.Adding a books to the list with non-existent number
        //BUG -> even if the user is authorized, the server is returning "Unauthorized" error. I assume that the correct expected result should be "Bad Request"
        [Test]
        [Category("Task 4")]
        public void Test_AddingNonExistentBook_ToList()
        {
            var request = new RestRequest("/bookstore/v1/books", Method.Post);

            request.AddHeader("Content-Type", "application/json");

            var userId = "2ed75802-de8a-4652-9a2f-cfd00c0a3bcb";

            var addListOfBooks = new
            {
                userId = userId,
                collectionOfIsbns = new List<Books>{
                     new Books{ isbn = "9781449862" } //not real number
                }

            };

            request.AddBody(addListOfBooks);

            var response = client.Execute(request);

            var issue = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(response.Content);

            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.That(issue.code, Is.Not.Empty);
            Assert.That(issue.message, Is.Not.Empty);
            //based on specification the "code" value should be returned as an "int" data type
            Assert.That(issue.code, Is.InstanceOf<int>());
            Assert.That(issue.message, Is.InstanceOf<string>());


        }

        //BUG -> even if the user is authorized, the server is returning "Unauthorized" error. Based on the specifications, the correct response should be "200"
        [Test]
        [Category("Task 5")]
        public void Test_ReplacingTheFirstBook_InList()
        {
            
            var request = new RestRequest("/bookstore/v1/books/9781449325862", Method.Put);

            request.AddHeader("Content-Type", "application/json");

            var userId = "2ed75802-de8a-4652-9a2f-cfd00c0a3bcb";

            var isbn = "9781593277574";

            var lastBookInList = new
            {
                userId = userId,
                isbn = isbn           
            };

            request.AddBody(lastBookInList);

            var response = client.Execute(request);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            var book = System.Text.Json.JsonSerializer.Deserialize<Books>(response.Content);

            var requestListOfBooks = new RestRequest("BookStore/v1/Books", Method.Get);

            var responseListOfBooks = client.Execute(requestListOfBooks);

            var bookWrapper = JsonConvert.DeserializeObject<BookWrapper>(responseListOfBooks.Content);

            List<Books> allBooks = bookWrapper.Books;

            Assert.That(allBooks[0], Is.Not.Empty);
            Assert.That(allBooks[0].isbn, Is.EqualTo(lastBookInList.isbn));
          

        }

        [Test]
        [Category("Task 6")]
        public void Test_ValidateTheNumberOfPagesOfABook()
        {
            var request = new RestRequest("BookStore/v1/Book?ISBN=9781491904244", Method.Get);

            request.AddHeader("Content-Type", "application/json");

            var response = client.Execute(request);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            var book = System.Text.Json.JsonSerializer.Deserialize<Books>(response.Content);

            int numberOfPages = book.pages;

            int expectedPages = 278;

            Assert.IsNotNull(book.pages);
            Assert.That(book.pages, Is.InstanceOf<int>());
            Assert.AreEqual(expectedPages, numberOfPages);

        }

        [Test]
        [Category("Task 7")]
        public void Test_RemovingABookFromTheList()
        {
            var request = new RestRequest("BookStore/v1/Book", Method.Delete);

            request.AddHeader("Content-Type", "application/json");

            var userId = "2ed75802-de8a-4652-9a2f-cfd00c0a3bcb";

            var isbn = "9781593275846";

            var deletedBook = new Books
            {
                userId = userId,
                isbn = isbn
            };

            request.AddBody(deletedBook);

            var response = client.Execute(request);

            Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
            
            var message = System.Text.Json.JsonSerializer.Deserialize<SuccessfulyDeletedResponse>(response.Content);

            Assert.That(message.message, Is.Not.Empty);
            Assert.That(message.isbn, Is.Not.Empty);
            Assert.That(message.userId, Is.Not.Empty);
            Assert.That(message.isbn, Is.EqualTo(deletedBook.isbn));
            Assert.That(message.userId, Is.EqualTo(deletedBook.userId));

            //Checking if the book is deleted from the list

            var requestListOfBooks = new RestRequest("BookStore/v1/Books", Method.Get);

            var responseListOfBooks = client.Execute(requestListOfBooks);          

            var bookWrapper = JsonConvert.DeserializeObject<BookWrapper>(responseListOfBooks.Content);

            List<Books> allBooks = bookWrapper.Books;

            for (int i = 0; i < allBooks.Count(); i++)
            {
                Assert.That(allBooks[i].isbn, Is.Not.EqualTo(deletedBook.isbn));
                Assert.IsNotEmpty(allBooks[i].isbn);
            }

        }

        [Test]
        [Category("Task 8")]
        public void Test_RemovingANonExistentBook_FromTheList()
        {
            var request = new RestRequest("BookStore/v1/Book", Method.Delete);

            request.AddHeader("Content-Type", "application/json");

            var userId = "2ed75802-de8a-4652-9a2f-cfd00c0a3bcb";

            var isbn = "9781593275846555151"; // non existent ISBN

            var deleteBook = new
            {
                userId = userId,
                isbn = isbn
            };

            request.AddBody(deleteBook);

            var response = client.Execute(request);

            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        }


    }

}
