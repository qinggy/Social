BEGIN
    --DECLARE @siteDbId INT
	DECLARE @siteFrom INT
	DECLARE @siteTo INT

	--SET @siteDbId=21;
	SET @siteFrom=(@siteDbId-1)*500
	SET @siteTo=@siteFrom+500

	DECLARE @siteId INT
	SET @siteId=@siteFrom

	WHILE @siteId < @siteTo
	BEGIN
	    DECLARE @siteIdStr nvarchar(100)
		SET @siteIdStr=CONVERT(nvarchar(100),@siteId)
		DECLARE @sql varchar(max)
		SET @sql=
		'
		INSERT INTO [t_Social_ConversationFieldOption]
		  (
			   [FieldId]
			  ,[Name]
			  ,[Value]
			  ,[Index]
			  ,[SiteId]
		  )
		 SELECT
		   (SELECT [Id]AS [FieldId] FROM [t_Social_ConversationField] WHERE [SiteId] = @siteIdStr AND [Name] = p.FieldName )
		      , [Name]
			  ,[Value]
			  ,[Index]
			  ,@siteIdStr AS [SiteId]
	 FROM [Comm100.General].[dbo].[t_Social_ConversationFieldOption_Config] p
	 '
	EXEC(@sql)
	SET @siteId=@siteId+1
	END
END