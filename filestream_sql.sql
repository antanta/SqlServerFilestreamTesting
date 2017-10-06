-- Orignal setting 0
-- Configuration option 'filestream access level' changed from 0 to 2. Run the RECONFIGURE statement to install.
EXEC sp_configure filestream_access_level, 2
GO
RECONFIGURE
GO 

CREATE TABLE [dbo].PictureTable
(
	PkId int Primary Key IDENTITY (1, 1),
	Id uniqueidentifier NOT NULL Unique ROWGUIDCOL Default newid(),
	Description nvarchar(64) NOT NULL,
	FileSummary varbinary(MAX),
	FileData varbinary(MAX) FileStream NULL
) 

Insert Into PictureTable([Description],[FileData])
Values('Hello World', Cast('Hello World' As varbinary(max)))

SELECT [PkId],[Id],[Description],[FileData],CAST([FileData] As varchar(Max)) FROM [PictureTable] 

--delete from [PictureTable] where PkId = 2

Select FileData.PathName() As Path From PictureTable Where PkId = 1