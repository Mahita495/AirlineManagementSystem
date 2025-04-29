AirlineManagementSystem  

It's an ASP.NET Core MVC Project with database MS SQL  
Tables included are Flights, Bookings and Users in same database AirlineDB

1. User initially registers and data will be stored in database. Password will be stored in encrypted format.  
2. User has to login using respective credentials, password will be hashed and compared with the one in DB.
3. If credentials match, a token is generated and stored in local storage and Cookies.This token is handshaked for every method to be accessible.  
4. After login, based on respective role a manager can Create, Edit, Update and Delete Flights and also book a flight.  
5. User can only view the details of the flight and book.
6. Both user and manager can view their respective list of bookings. 
   
