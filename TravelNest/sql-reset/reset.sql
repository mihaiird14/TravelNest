USE TravelNestDB;
GO
DBCC CHECKIDENT ('dbo.Postares', RESEED, 0);
DBCC CHECKIDENT ('dbo.FisierMedias', RESEED, 0);
DBCC CHECKIDENT ('dbo.TravelGroups', RESEED, 0);
DBCC CHECKIDENT ('dbo.LocatieGrups', RESEED, 0);
DBCC CHECKIDENT ('dbo.MembruGrups', RESEED, 0);


