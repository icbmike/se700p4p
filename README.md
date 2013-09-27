Auckland Transport - Traffic Reporter
========

Files Submitted

The source code on this CD contains the source code files which can be used to compile our Traffic Reporter software application. This application was included as part of our Part IV Honours project. Also, included on this CD are applications we used in the development of our project: the existing SCATS Traffic Reporter a binary file viewer and relevant documentation.


Traffic Reporter

Our Traffic Reporter reads binary Volume Store files and imports them into a local database. Our program has four modes: Home Mode which allows you to manage the data in your database, Report Mode which allows you to view traffic volumes in Graphical and Tabular views, Summary Mode which allows the user to see daily totals for peak periods and total traffic, and finally Faults Mode which lists detectors which are suspected to be faulty.


Dependencies

- SQLite, although in the future this should be swapped for SQL Server to aid performance
- C# with Windows Presentation Foundation framework


Interfaces

- IView is implemented by the view screens of all modes to allow for event handling
- IConfig is implemented by the configuration screens for all modes to allow for event handling


Installation Steps


