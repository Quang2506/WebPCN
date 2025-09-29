USE [PAL_COMPSN_P80]
GO

/****** Object:  StoredProcedure [dbo].[RFC_311AutoReturn_SendToSAP]    Script Date: 2025/8/13 16:57:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--select * from AutoReturn_DataToSAP order by Transdatetime desc
--select * from AutoReturn_DataToSAP where  status='SendFail' and Transdatetime>'20120831092519'

--EXEC RFC_311AutoReturn_SendToSAP 
--\\172.26.40.15\d$\PALFA\Share\UPLOAD\RFC\In
CREATE PROCEDURE [dbo].[RFC_311AutoReturn_SendToSAP] 
   --@PATH VARCHAR(200)='D:\PALFA\Share\UPLOAD\RFC\IN\'
   --@PATH VARCHAR(200)='d:\PALFA\Share\UPLOAD\RFC\In' 
   --@PATH VARCHAR(200)='\\172.26.40.26\d$\iMAC_Upload\RFC\In\' --0002 
   @PATH VARCHAR(200)='\\172.26.40.26\iMAC_Upload\RFC\In\' --0012
 AS
set nocount on
------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--*Program*: <AutoReturn>------------
--*Programer*:<Austin>
--*Date*:<2012/06/18>
--*Description*:<>
--*Unify*:<UA>
--########## Parameter Description Begin ##########
--########## Parameter Description End # ##########

--##########Update Log Begin ###################

--Date                 UpdateOwner            Description
--2012-06-29			Austin					将更新SendTime放在送资料的地方，以前是在接受到SAP资料的时候才更新的 ----00001
--20150211				Wes						更改RFC文件获取路径，从File Server上获取 (0002)
--20160602				Eric			        7天内1小时前sendfail数据重新传送 (0003)
--20160602              Eric                    添加Try…catch     (0004)
--20220525              Ira	                    优化     (0005)
--20220606				OuYang                  将原来的七天内重送改为1天内重送（0006）
--20220606				OuYang                  对SendFailde 扣账数据集重送SAP前，根据SAPErr在排除最近7天到最近1天的SendFail数据，不在重送，保留原SAPErr,Status更新为SendOK;(0007)
--20220614				OuYang					RFC的目錄結構/檔案數量與執行的效能	0008
--20220621              OuYang                  新增统计送SAP扣账逻辑  0009
--20220812				OuYang				   新增在维护的时间段内停止送SAP扣账数据的逻辑 （0010）
--20231228              Freedom                防止PackingInfoSend_AutoStock_SPT主键报错，加distinct和@StepFlag便于查找（0011）
--20240424              Joyce					QMS内部要求：RFC共享都已取消d$使用 0012  
--20250714              Tacey					应SAP要求文件名加Plant 0013  
--##########Update Log End # ###################
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------
DECLARE	
		@CurDateTime Varchar(20),
		@TransDatetime	VARCHAR(20),
		@FilePath		VARCHAR(200),
		@tempPath		VARCHAR(200),
		@sqlCmd			VARCHAR(8000),
		@LastDatetime	VARCHAR(20),
		@ExceptDatetime	VARCHAR(20),
		@referenceID	VARCHAR(50),
		@backUpPath		VARCHAR(200),
		@i				INT=0
		,@LastTime_1H	VARCHAR(50)=''	--0005

declare @Plant	VARCHAR(20)='' --0013  

----0004 begin 
declare @ErrMsg Nvarchar(4000)=''
declare @StepFlag varchar(100)=''
declare @SP_Name varchar(100)=''

Begin Try
	set @SP_Name=isnull(OBJECT_NAME(@@PROCID),'' )
	set @StepFlag='Try Start'
----0004 end
	--SET @backUpPath=Replace(@Path,'IN','BackUp')+'QMS\'
	--set @SP_Name='RFC_311AutoReturn_SendToSAP'
	exec File_Path_GetValue @SP_Name,'BakInPath', @backUpPath OutPut --- 0008
	--PRINT @backUpPath
	--print @SP_Name
	Set @CurDateTime = dbo.FormatDate(GETDATE(),'YYYYMMDDHHNNSS')
	SELECT @LastDatetime=dbo.FormatDate(GETDATE()-1,'YYYYMMDDHHNNSS')  --- 0006
		,@LastTime_1H = DBO.FormatDate(DATEADD(HOUR,-1,GETDATE()),'YYYYMMDDHHNNSS') --0005

	------ 0010 begin
	IF Exists(Select 1 from [SPT_SystemDisable_Maintain] where [SysName]=@SP_Name AND SysType='SP' AND @CurDateTime BETWEEN StartDT and EndDT and DisableFlag='Y')
	Begin
		print N'已经定义在此段时间内停止送扣账（无wo扣账）'
		Return
	End
	------ 0010 end
	--------- 0007 BEGIN
	Create table #ExceptionSendBySAPErr(SAPErr varchar(100))

	insert into #ExceptionSendBySAPErr
	values('already exist!Material Document:'),
			('is locked by the user')
	set @ExceptDatetime = dbo.FormatDate(GETDATE()-7,'YYYYMMDDHHNNSS')
	
	Update a set a.Status='SendOK' from AutoReturn_DataToSAP a,#ExceptionSendBySAPErr b 
	where a.SAPErr LIKE '%'+B.SAPErr+'%' and a.Transdatetime>@ExceptDatetime and a.Transdatetime<@LastDatetime

	----- 0007 END
	IF EXISTS(SELECT * FROM tempdb.dbo.sysobjects WHERE id = OBJECT_ID(N'tempdb.dbo.##AutoReturn_DataToSAP')) 
      BEGIN
		  DROP TABLE ##AutoReturn_DataToSAP
	   END

	CREATE TABLE [dbo].[##AutoReturn_DataToSAP](
		Header	VARCHAR(50),
		REFNO [varchar](50) NOT NULL,
		[Plant] [varchar](50) NOT NULL,
		MATNR [varchar](50) NOT NULL,
		CHARG [varchar](50) NULL,--NOT 
		KOSTL [varchar](50) NOT NULL,
		LGORTF [varchar](50) NOT NULL,
		LGORTT [varchar](50) NOT NULL,
		RETQTY [int] ,
		[Status] [varchar](50) NULL)--NOT
	
	CREATE TABLE [dbo].[#AutoReturn_DataToSAP_Temp](
		REFNO [varchar](50) NOT NULL,
		[Plant] [varchar](50) NOT NULL,
		PN [varchar](50) NOT NULL,
		REV [varchar](50)  NULL,--NOT
		CostCenter [varchar](50) NOT NULL,
		FromStoredLocation [varchar](50) NOT NULL,
		ToStoredLocation [varchar](50) NOT NULL,
		Qty [int])
	
	SET @i=0
	
	
	IF RIGHT(@Path,1)<>'\'
	BEGIN
		SET @Path=@Path+'\'
	END
	
	
	INSERT INTO #AutoReturn_DataToSAP_Temp(REFNO,Plant,PN,REV,CostCenter,FromStoredLocation,ToStoredLocation,Qty)
	SELECT ReferenceID,Plant,PN,Rev,CostCenter,FromStoredLocation,ToStoredLocation,Qty
	FROM AutoReturn_DataToSAP WITH(NOLOCK)
	WHERE Status IN('','N') AND Transdatetime>@LastDatetime
	
	------------------------------------- 0003 begin ----------------------------------
	INSERT INTO #AutoReturn_DataToSAP_Temp(REFNO,Plant,PN,REV,CostCenter,FromStoredLocation,ToStoredLocation,Qty)
	SELECT ReferenceID,Plant,PN,Rev,CostCenter,FromStoredLocation,ToStoredLocation,Qty
	FROM AutoReturn_DataToSAP WITH(NOLOCK)
	WHERE Status in('SendFail') AND Transdatetime>@LastDatetime 
		--AND SendTime<DBO.FormatDate(DATEADD(HOUR,-1,GETDATE()),'YYYYMMDDHHNNSS')	--0005
		AND SendTime<@LastTime_1H		--0005

	------------------------------------- 0003  end -----------------------------------
	--select * from #AutoReturn_DataToSAP_Temp
	IF NOT EXISTS(SELECT 1 FROM #AutoReturn_DataToSAP_Temp)
	BEGIN
		RETURN
	END 
	----已经送过的PalletiD  --- 0009 BEGIN
    delete A from #AutoReturn_DataToSAP_Temp A where exists(
	   Select B.* from PackingInfoSend_AutoStock_SPT B Where A.REFNO = B.PalletID and B.Status='S')----0011

	select * into #AutoReturn_DataToSAP_Temp_BK from #AutoReturn_DataToSAP_Temp

	Select B.* into #TempOneSendPacking from #AutoReturn_DataToSAP_Temp A,PackingInfoSend_AutoStock_SPT B Where A.REFNO = B.PalletID

	Create table #ReferenceIDMax (PalletID VARCHAR(30),SendTimes int) 
	 insert into #ReferenceIDMax(PalletID,SendTimes)
	 Select PalletID,max(SendTimes) from #TempOneSendPacking Group by PalletID
	 ---取出送过1/2次的PalletID
	 Select * into #ReferenceIDAddOne from #ReferenceIDMax where SendTimes<3
	 ---剩余送过3次的
	 delete from #ReferenceIDMax where SendTimes<3

	 set @StepFlag='Insert'
	If Exists(Select 1 from #AutoReturn_DataToSAP_Temp_BK )
	Begin
		---- 第一次送扣账的记录一次
		Delete A from #AutoReturn_DataToSAP_Temp_BK A,#TempOneSendPacking B where A.REFNO=B.PalletID
		Insert into PackingInfoSend_AutoStock_SPT
		Select distinct REFNO,1,'','P','Auto',@CurDateTime,'' -----0011 add distinct
		from #AutoReturn_DataToSAP_Temp_BK
	End
	set @StepFlag='Update2'
	If Exists(Select 1 from #ReferenceIDAddOne)
	Begin
		---- 送过1/2次的还能重送；
		Update A Set A.SendTimes = A.SendTimes+1 ,A.FailReason='',A.TransDateTime=@CurDateTime
		from  PackingInfoSend_AutoStock_SPT A,#ReferenceIDAddOne B where A.PalletID=B.PalletID and A.SendTimes<3
		insert into #AutoReturn_DataToSAP_Temp_BK
		Select A.* from #AutoReturn_DataToSAP_Temp A,#ReferenceIDAddOne B where A.REFNO=B.PalletID and B.SendTimes<3
	End
	set @StepFlag='Update3'
	If Exists(Select 1 from #ReferenceIDMax)
	Begin
		--- 对于已经送过3次的若有PackingInfoSend_AutoStock_SPT 最近一笔Status='R'，表示仓库手动重送
		Select *,ROW_NUMBER() OVER(Partition by PalletID Order by TransDateTime desc) as Num
		into #tempSendPacking
		FROM #TempOneSendPacking
		Select * into #SendtoSAP FROM #tempSendPacking WHERE Num=1 and [Status]='R'
		Insert into #AutoReturn_DataToSAP_Temp_BK
		Select A.* from #AutoReturn_DataToSAP_Temp A,#SendtoSAP  B 
		where A.REFNO=B.PalletID 

		Update A Set A.SendTimes = A.SendTimes+1 ,A.FailReason='',A.Status='P',A.TransDateTime=@CurDateTime,A.[UID]='Manual'
		from  PackingInfoSend_AutoStock_SPT A,#SendtoSAP B where A.PalletID=B.PalletID and A.[Status]='R'
	End
	truncate table #AutoReturn_DataToSAP_Temp
	insert into #AutoReturn_DataToSAP_Temp
	select * from #AutoReturn_DataToSAP_Temp_BK
	------------------------   0009 END
	WHILE EXISTS(SELECT * FROM #AutoReturn_DataToSAP_Temp)
	BEGIN
		TRUNCATE TABLE ##AutoReturn_DataToSAP
		
		
		SET @TransDatetime=dbo.FormatDate(GETDATE(),'YYYYMMDDHHNNSS')
		--------0013  -1 begin
		--SET @FilePath='QMS_311AutoReturn_'+@TransDatetime+CAST(@i AS VARCHAR)+'.txt'
		--SET @tempPath='QMS_311AutoReturn_'+@TransDatetime+CAST(@i AS VARCHAR)+'.tmp'
	
		--SET @Path=@Path+@tempPath
		--------0013  -1 end
		INSERT INTO ##AutoReturn_DataToSAP(Header,REFNO,Plant,MATNR,KOSTL,LGORTF,LGORTT,RETQTY,CHARG,Status)
		SELECT TOP 1
			'<ITABIN>',
			REFNO,
			Plant,
			PN as MATNR,
			CASE WHEN CostCenter='' THEN ' ' ELSE CostCenter END AS KOSTL,
			FromStoredLocation AS LGORTF,
			ToStoredLocation AS LGORTT,
			cast(Qty as Decimal) AS RETQTY,
			----updated by susie on 2013/4/12
			case when REV='' then null
			      else REV END AS CHARG,
			--'' as Status,
			 NULL AS Status
			-----------------------------------
		FROM #AutoReturn_DataToSAP_Temp with(nolock) 
		
		SELECT @referenceID=REFNO,@Plant=Plant FROM ##AutoReturn_DataToSAP  ---0013增加获取plant
			------0013  -2 begin 与Pei-Hua议定更改QMS_311AutoReturn为QMS_311AR
		
		SET @FilePath='QMS_311AR'+@Plant+'_'+@TransDatetime+CAST(@i AS VARCHAR)+'.txt'
		SET @tempPath='QMS_311AR'+@Plant+'_'+@TransDatetime+CAST(@i AS VARCHAR)+'.tmp'
	
		SET @Path=@Path+@tempPath
		------0013  -2 end
		
		UPDATE AutoReturn_DataToSAP 
		SET SendTime=@TransDatetime,Status='Sending' 
		WHERE ReferenceID=@referenceID
	
		INSERT INTO ##AutoReturn_DataToSAP(Header,REFNO,Plant,MATNR,KOSTL,LGORTF,LGORTT,RETQTY,CHARG,Status)
		SELECT '<RECORD>','','','','','','',NULL,'',''
		----0002
		Set @sqlCmd= 'BCP "Select Header,REFNO,Plant, MATNR, KOSTL, LGORTF, LGORTT,RETQTY,CHARG,STATUS From PAL_COMPSN_P80.DBO.##AutoReturn_DataToSAP ORDER BY HEADER desc" QueryOut '+@Path+' -c -T'
		
		EXEC master..xp_cmdshell @sqlCmd
		
		INSERT INTO RFC_FileRecord(Name,RFCName,RequestFile,CreateDatetime,Status,ResultFile,FinishDatetime)
		   SELECT '311AutoReturn','Zrfc_311_Auto_Return',@FilePath,@TransDatetime,'00','',''
		
		SET @sqlCmd='COPY '+@Path+' '+@backUpPath+@FilePath
		EXEC MASTER..XP_CMDSHELL @sqlCmd
		--print @sqlCmd
		SET @sqlCmd='rename '+@Path+' '+@FilePath
		EXEC master..XP_CMDSHELL @SQLCMD 
	
		DELETE FROM #AutoReturn_DataToSAP_Temp WHERE REFNO=@referenceID
		
		SET @i=@i+1
	END
-----0004 begin 
    set @StepFlag='Try End'

End Try
Begin Catch

	Select @ErrMsg=N'Stored Precedure:['+@SP_Name+'] is executed exceptionally,'
		+N'ERROR_NUMBER:'+CAST(ERROR_NUMBER() as varchar(50)) 
		+N'ERROR_MESSAGE:'+ERROR_MESSAGE() 
	 
	set @StepFlag=@StepFlag+':' +'Error Happen'

	exec MonitorAgent_SaveErrorLog @SP_Name,@PATH,'',@StepFlag,@ErrMsg
	 
End Catch	
-----0004 end	

 




GO


