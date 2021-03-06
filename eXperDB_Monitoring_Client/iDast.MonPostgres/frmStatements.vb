﻿Public Class frmStatements

    Private _InstanceID As Integer = -1
    Private _SelectedIndex As String
    Private _SelectedGrid As String
    Private _AreaCount As Integer = 4
    Private _chtCount As Integer = 0
    Private _chtHeight As Integer
    Private _clsQuery As clsQuerys
    Private _bChartMenu As Boolean = False
    Private _bRange As Boolean = False
    Private _annotationIndex As Integer = -1
    'Private _strTopQueryIDs = ""
    'Private _strTopQueryDBIDs = ""
    Private _arrQueryIDs As New ArrayList
    Private _arrDBIDs As New ArrayList

    Private _ThreadDetail As Threading.Thread
    Private _ListStatements As New List(Of String)
    Private _UseFilter As Boolean


    ReadOnly Property InstanceID As Integer
        Get
            Return _InstanceID
        End Get
    End Property
    Private _SvrpList As List(Of GroupInfo.ServerInfo)
    Private _ServerInfo As GroupInfo.ServerInfo = Nothing

    Private _AgentInfo As structAgent
    ReadOnly Property AgentInfo As structAgent
        Get
            Return _AgentInfo
        End Get
    End Property
    Private _AgentCn As eXperDB.ODBC.eXperDBODBC

    ReadOnly Property AgentCn As eXperDBODBC
        Get
            Return _AgentCn
        End Get
    End Property

    'Private _queryColors() As Color = {System.Drawing.Color.YellowGreen,
    '                 System.Drawing.Color.Salmon,
    '                 System.Drawing.Color.Violet,
    '                 System.Drawing.Color.Aqua,
    '                 System.Drawing.Color.SpringGreen,
    '                 System.Drawing.Color.SkyBlue,
    '                 System.Drawing.Color.Yellow,
    '                 System.Drawing.Color.Pink,
    '                 System.Drawing.Color.Purple,
    '                 System.Drawing.Color.PowderBlue,
    '                 System.Drawing.Color.Green,
    '                 System.Drawing.Color.Brown,
    '                 System.Drawing.Color.Blue,
    '                 System.Drawing.Color.Red,
    '                 System.Drawing.Color.Orange,
    '                 System.Drawing.Color.FromArgb(255, CType(CType(0, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(192, Byte), Integer))}
    Private _queryColors() As Color = {System.Drawing.Color.OrangeRed,
                     System.Drawing.Color.Orange,
                     System.Drawing.Color.Gold,
                     System.Drawing.Color.YellowGreen,
                     System.Drawing.Color.SkyBlue,
                     System.Drawing.Color.DodgerBlue,
                     System.Drawing.Color.Violet,
                     System.Drawing.Color.Pink,
                     System.Drawing.Color.Purple,
                     System.Drawing.Color.Tan,
                     System.Drawing.Color.SpringGreen,
                     System.Drawing.Color.Brown,
                     System.Drawing.Color.Blue,
                     System.Drawing.Color.Red,
                     System.Drawing.Color.Aqua,
                     System.Drawing.Color.FromArgb(255, CType(CType(0, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(192, Byte), Integer))}

    Public Sub New(ByVal AgentCn As eXperDB.ODBC.eXperDBODBC, ByVal ServerInfo As List(Of GroupInfo.ServerInfo), ByVal intInstanceID As Integer, ByVal stDt As DateTime, ByVal edDt As DateTime, ByVal AgentInfo As structAgent)

        ' 이 호출은 디자이너에 필요합니다.
        InitializeComponent()

        ' InitializeComponent() 호출 뒤에 초기화 코드를 추가하십시오.

        _InstanceID = intInstanceID
        _SvrpList = ServerInfo
        _AgentInfo = AgentInfo
        _AgentCn = AgentCn

        _clsQuery = New clsQuerys(_AgentCn)
        For Each tmpSvr As GroupInfo.ServerInfo In _SvrpList
            If tmpSvr.InstanceID = _InstanceID Then
                _ServerInfo = tmpSvr
            End If
            cmbInst.AddValue(tmpSvr.InstanceID, tmpSvr.ShowNm)
        Next

        dtpSt.Value = stDt.AddMinutes(0)
        dtpEd.Value = edDt.AddMinutes(0)
        dtpSt.Tag = stDt
        dtpEd.Tag = edDt

        dtpSt.Visible = True
        dtpEd.Visible = True
        lblSt.Visible = False
        lblEd.Visible = False
        btnQuery.Visible = True

        InitForm()
        InitCharts()
        ReadConfig()
    End Sub

    ''' <summary>
    ''' 화면 초기화 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmStatements_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _InstanceID > 0 Then
            Dim comboSource As New Dictionary(Of String, String)()
            Dim index As Integer = 0
            For Each tmpSvr As GroupInfo.ServerInfo In _SvrpList
                If tmpSvr.InstanceID = _InstanceID Then
                    cmbInst.SelectedIndex = index
                    If cmbInst.SelectedIndex = 0 Then
                        Me.Invoke(New MethodInvoker(Sub()
                                                        btnQuery.PerformClick()
                                                    End Sub))
                    End If
                End If
                index += 1
            Next
        End If

        chtCalls.Height = 1160 'fix Height of Chart
        chtCalls.MainChart.Focus()

        '_chtCount = 1
        chtCalls.MainChart.Focus()

        'btnQuery.PerformClick()
    End Sub

    Public Sub frmStatements_ReLoad(ByVal intInstanceID As Integer, ByVal stDt As DateTime, ByVal edDt As DateTime)
        Dim prevStdt As DateTime = dtpSt.Value
        Dim prevInstanceIndex = cmbInst.SelectedIndex

        chtCalls.MainChart.Focus()

        _InstanceID = intInstanceID

        dtpSt.Value = stDt.AddMinutes(0)
        dtpEd.Value = edDt.AddMinutes(0)
        dtpSt.Tag = stDt
        dtpEd.Tag = edDt

        cmbInst.SelectedValue = intInstanceID

        rb1H.Checked = False
        rb2H.Checked = False
        rb4H.Checked = False
        rb12H.Checked = False
        rb1D.Checked = False

        If prevInstanceIndex = cmbInst.SelectedIndex AndAlso prevStdt <> dtpSt.Value Then
            Me.Invoke(New MethodInvoker(Sub()
                                            btnQuery.PerformClick()
                                        End Sub))
        End If
    End Sub

    Private Sub InitForm()

        Dim strHeader As String = Common.ClsConfigure.fn_rtnComponentDescription(p_ShowName.GetType.GetMember(p_ShowName.ToString)(0))
        Me.Text += " [ " + String.Format("{0}({1}) ", _ServerInfo.ShowNm, _ServerInfo.IP) + "]"
        lblSubject.Text = p_clsMsgData.fn_GetData("M059")
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'label & Input
        lblServer.Text = p_clsMsgData.fn_GetData("F033")
        lblDuration.Text = p_clsMsgData.fn_GetData("F254")
        lblChart.Text = p_clsMsgData.fn_GetData("F324")

        ' Button 
        btnQuery.Text = p_clsMsgData.fn_GetData("F151")
        btnRange.Text = p_clsMsgData.fn_GetData("F269", "Off")

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' Talble Information (whole)

        lslSession.Text = p_clsMsgData.fn_GetData("F323", 0)
        lblSort.Text = p_clsMsgData.fn_GetData("F325")
        cbxHideSysSQL.Text = p_clsMsgData.fn_GetData("F331")
        btnEditFiltering.Text = p_clsMsgData.fn_GetData("F333")
        cmbSort.SelectedIndex = 0
        cmbTop.SelectedIndex = 0
        dgvStmtList.AutoGenerateColumns = False
        dgvStmtList.DefaultCellStyle.Font = New System.Drawing.Font("Gulim", 9.0!)
        dgvStmtList.ColumnHeadersDefaultCellStyle.Font = New System.Drawing.Font("Gulim", 9.0!)


        '''''''''''''''''''''''''''''''''''''''''
        chtCalls.Visible = False
        chtTotalTime.Visible = False
        chtIOTime.Visible = False
        chtCPUTime.Visible = False

        'modCommon.FontChange(Me, p_Font)
    End Sub

    Private Sub sb_AddTreeGridDatas(ByVal tvNode As AdvancedDataGridView.TreeGridNode, ByVal ColHashSet As Hashtable, ByVal DtRow As DataRow)
        For Each tmpColIdx As Integer In ColHashSet.Keys
            tvNode.Cells(tmpColIdx).Value = DtRow.Item(ColHashSet.Item(tmpColIdx))
        Next

    End Sub

    ''' <summary>
    ''' BackEnd 정보 등록 
    ''' </summary>
    ''' <param name="starDt"></param>
    ''' <param name="endDt"></param>
    ''' <remarks></remarks>
    Public Sub SetDataSession(ByVal starDt As DateTime, ByVal endDt As DateTime)

        Dim dtTable As DataTable = Nothing
        Dim tmpTh As Threading.Thread = New Threading.Thread(Sub()
                                                                 Try
                                                                     dtTable = _clsQuery.SelectStatementsList(_InstanceID, starDt, endDt)
                                                                 Catch ex As Exception
                                                                     GC.Collect()
                                                                 End Try
                                                             End Sub)
        tmpTh.Start()
        tmpTh.Join()
        If dtTable IsNot Nothing Then
            Me.Invoke(New MethodInvoker(Sub()
                                            Try
                                                Dim strQuery As String = ""

                                                strQuery = String.Format("INSTANCE_ID = {0}", Me.InstanceID)

                                                Dim dtView As DataView = dtTable.AsEnumerable.AsDataView

                                                Dim ShowDT As DataTable = Nothing
                                                If dtView.Count > 0 Then
                                                    ShowDT = dtView.ToTable.AsEnumerable.Take(200).CopyToDataTable
                                                End If

                                                If ShowDT Is Nothing Then
                                                    dgvStmtList.DataSource = Nothing
                                                    Return
                                                End If

                                                STMTTableBindingSource.DataSource = ShowDT

                                                modCommon.sb_GridSortChg(dgvStmtList)
                                                lslSession.Text = p_clsMsgData.fn_GetData("F323", dtView.Count)

                                            Catch ex As Exception
                                                p_Log.AddMessage(clsLog4Net.enmType.Error, ex.ToString)
                                                GC.Collect()
                                            End Try

                                        End Sub))
        End If
    End Sub

    Private Sub InitCharts()

        _chtHeight = chtCalls.Height + 30

        chtCalls.MainChart.ChartAreas(0).Visible = False
        chtCalls.AddAreaEx("Calls", "Count", True, "CALLAREA", False)
        chtCalls.AddAreaEx("Total time", "MilliSEC", True, "TOTALTIMEAREA", False)
        chtCalls.AddAreaEx("CPU Time", "RATE[%]", True, "CPUTIMEAREA", False)
        chtCalls.AddAreaEx("IO Time", "RATE[%]", True, "IOTIMEAREA", False)

        chtCalls.MainChart.ChartAreas("CALLAREA").Visible = True
        chtCalls.MainChart.ChartAreas("TOTALTIMEAREA").Visible = True
        chtCalls.MainChart.ChartAreas("CPUTIMEAREA").Visible = True
        chtCalls.MainChart.ChartAreas("IOTIMEAREA").Visible = True

        Me.chtCalls.Visible = True

        chtCalls.MainChart.ChartAreas("CALLAREA").CursorX.IntervalType = DataVisualization.Charting.DateTimeIntervalType.Seconds
        chtCalls.MainChart.ChartAreas("CALLAREA").CursorX.IntervalOffsetType = DataVisualization.Charting.DateTimeIntervalType.Seconds

        chtCalls.MainChart.ChartAreas("TOTALTIMEAREA").CursorX.IntervalType = DataVisualization.Charting.DateTimeIntervalType.Seconds
        chtCalls.MainChart.ChartAreas("TOTALTIMEAREA").CursorX.IntervalOffsetType = DataVisualization.Charting.DateTimeIntervalType.Seconds

        chtCalls.MainChart.ChartAreas("CPUTIMEAREA").CursorX.IntervalType = DataVisualization.Charting.DateTimeIntervalType.Seconds
        chtCalls.MainChart.ChartAreas("CPUTIMEAREA").CursorX.IntervalOffsetType = DataVisualization.Charting.DateTimeIntervalType.Seconds

        chtCalls.MainChart.ChartAreas("IOTIMEAREA").CursorX.IntervalType = DataVisualization.Charting.DateTimeIntervalType.Seconds
        chtCalls.MainChart.ChartAreas("IOTIMEAREA").CursorX.IntervalOffsetType = DataVisualization.Charting.DateTimeIntervalType.Seconds
    End Sub

    Private Sub QueryChartData(ByVal index As Integer, ByVal enable As Boolean)
        SetTopQueryIDs(index)
        ShowDynamicChart(index, dtpSt.Value, dtpEd.Value, enable)
    End Sub

    Private Sub ArrangeChartlayout()
        Dim tmpChartArea As System.Windows.Forms.DataVisualization.Charting.ChartArea
        Dim nCount As Integer = 0
        Dim MarginTop As Double = 0
        Dim MarginBottom As Double = 0
        Dim AreaHeight As Double = (100 / 4)
        MarginTop = AreaHeight * 0.18
        MarginBottom = AreaHeight * 0.18
        AreaHeight = AreaHeight * 0.64
        For i As Integer = 1 To _AreaCount
            tmpChartArea = Me.chtCalls.MainChart.ChartAreas(i)
            If tmpChartArea.Visible = True Then
                tmpChartArea.Position.Y = (nCount * AreaHeight) + MarginTop * (nCount + 1) + MarginBottom * nCount
                tmpChartArea.Position.Height = AreaHeight
                tmpChartArea.Position.X = 3
                If i = 3 AndAlso tmpChartArea.Position.Width < 90 Then
                    tmpChartArea.Position.Width = tmpChartArea.Position.Width * (1 + CSng(100 / (Me.chtCalls.MainChart.Width)))
                End If
                nCount += 1
            End If
        Next
        SetAreaWithAnnotation()
    End Sub

    Private Sub SetAreaWithAnnotation()
        If _bRange = True Then
            Dim vlStart As DataVisualization.Charting.VerticalLineAnnotation = chtCalls.MainChart.Annotations(0)
            Dim vlEnd As DataVisualization.Charting.VerticalLineAnnotation = chtCalls.MainChart.Annotations(1)
            For Each tmpChartArea As DataVisualization.Charting.ChartArea In chtCalls.MainChart.ChartAreas
                If tmpChartArea.Visible = True Then
                    tmpChartArea.CursorX.SetSelectionPosition(vlStart.X, vlEnd.X)
                End If
            Next
        End If
    End Sub

    Private Sub ShowDynamicChart(ByVal index As Integer, ByVal stDate As DateTime, ByVal edDate As DateTime, ByVal ShowChart As Boolean)
        Dim strSeriesData1 As String = ""
        Dim strSeriesData2 As String = ""
        Dim strSeriesData3 As String = ""
        Dim strSeriesName As String = ""
        Dim strPrevData As New ArrayList

        Dim seriesChartType As DataVisualization.Charting.SeriesChartType = DataVisualization.Charting.SeriesChartType.SplineArea

        Dim yAxisType As DataVisualization.Charting.AxisType = DataVisualization.Charting.AxisType.Primary

        Dim arrColors As New ArrayList
        Dim arrIndex As Integer = 0

        'Dim Queries As Long() = _arrQueryIDs.ToArray(GetType(Long))
        'Dim DBs As Long() = _arrDBIDs.ToArray(GetType(Long))
        Dim strTopQueryIDs = ""
        Dim strTopQueryIDsOrder = "CASE PGS.DBID::TEXT || QUERYID::TEXT "

        For j As Integer = 0 To _arrQueryIDs.Count - 1
            arrColors.Add(_queryColors(arrIndex))
            strPrevData.Add(-1)
            arrIndex += 1
            If j = _arrQueryIDs.Count - 1 Then
                strTopQueryIDs += "'" + _arrDBIDs(j).ToString + _arrQueryIDs(j).ToString + "'"
                strTopQueryIDsOrder += "WHEN '" + _arrDBIDs(j).ToString + _arrQueryIDs(j).ToString + "' THEN " + j.ToString + " END"
            Else
                strTopQueryIDs += "'" + _arrDBIDs(j).ToString + _arrQueryIDs(j).ToString + "',"
                strTopQueryIDsOrder += "WHEN '" + _arrDBIDs(j).ToString + _arrQueryIDs(j).ToString + "' THEN " + j.ToString + " "
            End If
        Next

        strTopQueryIDs = String.Join(",", strTopQueryIDs)

        strSeriesData1 = "QUERYID"
        strSeriesData2 = "DBID"

        Select Case index
            Case 1
                strSeriesData3 = "CALLS"
            Case 2
                strSeriesData3 = "TOTAL_TIME"
            Case 3
                strSeriesData3 = "CPU_TIME"
            Case 4
                strSeriesData3 = "IO_TIME"
        End Select

        Me.Invoke(New MethodInvoker(Sub()
                                        Try
                                            For Each tmpSeries As DataVisualization.Charting.Series In chtCalls.MainChart.Series
                                                If tmpSeries.ChartArea = chtCalls.MainChart.ChartAreas(index).Name Then
                                                    tmpSeries.Points.Clear()
                                                End If
                                            Next
                                            ArrangeChartlayout()
                                        Catch ex As Exception
                                            GC.Collect()
                                        End Try
                                    End Sub))

        Me.Invoke(New MethodInvoker(Sub()
                                        Try
                                            arrIndex = 0
                                            For Each tmpStr As String In _arrQueryIDs
                                                strSeriesName = strSeriesData3 + "_" + _arrDBIDs(arrIndex).ToString + "_" + tmpStr
                                                If chtCalls.GetSeries(strSeriesName) = False Then
                                                    chtCalls.AddSeries(chtCalls.MainChart.ChartAreas(index).Name, strSeriesName, tmpStr, arrColors(arrIndex), seriesChartType)
                                                End If
                                                arrIndex += 1
                                            Next
                                        Catch ex As Exception
                                            GC.Collect()
                                        End Try
                                    End Sub))

        Dim dtTable As DataTable = Nothing
        Dim tmpTh As Threading.Thread = New Threading.Thread(Sub()
                                                                 Try
                                                                     dtTable = _clsQuery.SelectStatementsCalls(_InstanceID, stDate, edDate, strTopQueryIDs, strTopQueryIDsOrder)
                                                                 Catch ex As Exception
                                                                     GC.Collect()
                                                                 End Try
                                                             End Sub)
        tmpTh.Start()
        tmpTh.Join()
        If dtTable IsNot Nothing Then
            Me.Invoke(New MethodInvoker(Sub()
                                            Try
                                                For Each tmpSeries As DataVisualization.Charting.Series In chtCalls.MainChart.Series
                                                    If tmpSeries.Name.Contains(strSeriesData3) = True Then
                                                        tmpSeries.Points.Clear()
                                                    End If
                                                Next

                                                'For i = 1 To _AreaCount
                                                '    chtCalls.SetMinimumAxisXChartArea(ConvOADate(stDate), i)
                                                '    chtCalls.SetMaximumAxisXChartArea(ConvOADate(edDate), i)
                                                'Next
                                                Dim sDateCollect As Double = 0.0
                                                If dtTable.Rows.Count > 0 Then
                                                    For i As Integer = 0 To dtTable.Rows.Count - 1
                                                        Dim tmpDate As Double = ConvOADate(dtTable.Rows(i).Item("COLLECT_DT"))
                                                        Dim j As Integer = 0
                                                        For Each tmpStr As String In _arrQueryIDs
                                                            If dtTable.Rows(i).Item(strSeriesData1) = tmpStr _
                                                            AndAlso dtTable.Rows(i).Item(strSeriesData2) = _arrDBIDs(j) Then
                                                                'strSeriesName = strSeriesData3 + tmpStr
                                                                strSeriesName = strSeriesData3 + "_" + _arrDBIDs(j).ToString + "_" + tmpStr
                                                                If strPrevData(j) >= 0 Then
                                                                    Me.chtCalls.AddPoints(strSeriesName, tmpDate, dtTable.Rows(i).Item(strSeriesData3) - strPrevData(j))
                                                                    If sDateCollect = 0.0 Then
                                                                        sDateCollect = tmpDate
                                                                    End If
                                                                End If
                                                                If index < 3 Then
                                                                    strPrevData(j) = dtTable.Rows(i).Item(strSeriesData3)
                                                                Else
                                                                    strPrevData(j) = 0
                                                                End If
                                                            End If
                                                            j += 1
                                                        Next
                                                    Next
                                                Else
                                                    Dim tmpDate As Double = ConvOADate(Now())
                                                    Dim j As Integer = 0
                                                    For Each tmpStr As String In _arrQueryIDs
                                                        'strSeriesName = strSeriesData3 + tmpStr
                                                        strSeriesName = strSeriesData3 + "_" + _arrDBIDs(j).ToString + "_" + tmpStr
                                                        Me.chtCalls.AddPoints(tmpStr, strSeriesName, 0.0)
                                                        j += 1
                                                    Next
                                                End If

                                                For i = 1 To _AreaCount
                                                    chtCalls.SetMinimumAxisXChartArea(sDateCollect, i)
                                                    chtCalls.SetMaximumAxisXChartArea(ConvOADate(edDate), i)
                                                Next

                                                Me.chtCalls.ShowMaxValue(True)
                                                chtCalls.MainChart.ChartAreas(index).RecalculateAxesScale()
                                            Catch ex As Exception
                                                p_Log.AddMessage(clsLog4Net.enmType.Error, ex.ToString)
                                                GC.Collect()
                                            End Try

                                        End Sub))
        End If

        Me.Invoke(New MethodInvoker(Sub()
                                        chtCalls.SetInnerPlotPositionChartArea(index, _chtCount)
                                        chtCalls.MainChart.ChartAreas(index).RecalculateAxesScale()
                                        ArrangeChartlayout()
                                    End Sub))

    End Sub


    Private Sub frmStatements_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        _clsQuery.CancelCommand()
        If _ThreadDetail IsNot Nothing Then
            _ThreadDetail.Abort()
            _ThreadDetail = Nothing
        End If
    End Sub

    Private Sub btnQuery_Click(sender As Object, e As EventArgs) Handles btnQuery.Click
        txtQueryID.Text = ""
        txtSQL.Text = ""
        Try
            chtCalls.SeriesClear()

            If _bRange = True Then
                fn_DeleteAnnotaion()
            End If

            If fn_SearchBefCheck() = False Then Return
        Catch ex As Exception
            GC.Collect()
        End Try
        For i As Integer = 1 To _AreaCount
            If chtCalls.MainChart.ChartAreas(i).Visible = True Then
                QueryChartData(i, True)
            End If
        Next
        SetDataSession(dtpSt.Value, dtpEd.Value)
        cbxHideSysSQL.Checked = _UseFilter
        'SetRowfilter(cbxHideSysSQL)
    End Sub

    Private Sub btnRange_Click(sender As Object, e As EventArgs) Handles btnRange.Click
        If _bRange = True Then
            fn_DeleteAnnotaion()
            SetDataSession(dtpSt.Value, dtpEd.Value)
        Else
            Dim index As Integer
            For index = 1 To _AreaCount
                If chtCalls.MainChart.ChartAreas(index).Visible = True Then
                    Exit For
                End If
            Next
            fn_MakeAnnotation(index)
            _annotationIndex = index
            _bRange = True
        End If
    End Sub

    Private Sub chtCPU_AnnotationPositionChanged(sender As Object, e As EventArgs)
        Dim vlStart As DataVisualization.Charting.VerticalLineAnnotation = chtCalls.MainChart.Annotations(0)
        Dim vlEnd As DataVisualization.Charting.VerticalLineAnnotation = chtCalls.MainChart.Annotations(1)
        Dim index As Integer = -1
        For index = 1 To _AreaCount
            If chtCalls.MainChart.ChartAreas(index).Visible = True Then
                Exit For
            End If
        Next

        SetAreaWithAnnotation()

        If chtCalls.MainChart.Annotations(0).X < chtCalls.GetMinimumAxisXChartArea(index) _
            Or chtCalls.MainChart.Annotations(0).X > chtCalls.GetMaximumAxisXChartArea(index) Then
            chtCalls.MainChart.Annotations(0).X = chtCalls.GetMinimumAxisXChartArea(index)
        End If

        If chtCalls.MainChart.Annotations(1).X < chtCalls.GetMinimumAxisXChartArea(index) _
            Or chtCalls.MainChart.Annotations(1).X > chtCalls.GetMaximumAxisXChartArea(index) Then
            chtCalls.MainChart.Annotations(1).X = chtCalls.GetMaximumAxisXChartArea(index)
        End If

        SetDataSession(DateTime.FromOADate(vlStart.X), DateTime.FromOADate(vlEnd.X))
    End Sub

    Private Sub chtCPU_AnnotationPositionChanging(sender As Object, e As EventArgs)
        Dim vl As DataVisualization.Charting.VerticalLineAnnotation = DirectCast(sender, DataVisualization.Charting.VerticalLineAnnotation)
        Dim vlStart As DataVisualization.Charting.VerticalLineAnnotation = chtCalls.MainChart.Annotations(0)
        Dim vlEnd As DataVisualization.Charting.VerticalLineAnnotation = chtCalls.MainChart.Annotations(1)

        If vl.Name = "StartTime" Then
            If vl.X >= vlEnd.X Then
                chtCalls.MainChart.Annotations(1).X = vl.X + (2 * 0.00003471)
            End If
        Else
            If vl.X <= vlStart.X Then
                chtCalls.MainChart.Annotations(0).X = vl.X - (2 * 0.00003471)
            End If
        End If

        SetAreaWithAnnotation()
    End Sub

    Private Sub dgvStmtList_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvStmtList.CellDoubleClick
        Dim strDb As String = ""
        Dim strUser As String = ""
        Dim strQuery As String = ""
        If dgvStmtList.RowCount <= 0 Then Return
        strDb = dgvStmtList.CurrentRow.Cells(coldgvStmtDBID.Index).Value
        strUser = IIf(IsDBNull(dgvStmtList.CurrentRow.Cells(coldgvStmtUserID.Index).Value), "", dgvStmtList.CurrentRow.Cells(coldgvStmtUserID.Index).Value)
        strQuery = IIf(IsDBNull(dgvStmtList.CurrentRow.Cells(coldgvStmtQuery.Index).Value), "", dgvStmtList.CurrentRow.Cells(coldgvStmtQuery.Index).Value)
        Dim frmQuery As New frmQueryView(strQuery, strDb, Me.InstanceID, Me.AgentInfo, strUser)
        frmQuery.ShowDialog(Me)
        'End If
    End Sub

    Private Function fn_SearchBefCheck() As Boolean
        If DateDiff(DateInterval.Minute, dtpSt.Value, dtpEd.Value) < 0 Then
            MsgBox(p_clsMsgData.fn_GetData("M014"))
            Return False
        Else
            If DateDiff(DateInterval.Minute, dtpSt.Value, dtpEd.Value) > 1440 Then
                MsgBox(p_clsMsgData.fn_GetData("M015", "1"))
                Return False
            End If
        End If
        Return True
    End Function

    Private Sub cmbInst_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbInst.SelectedIndexChanged
        _InstanceID = cmbInst.SelectedValue
        Me.Invoke(New MethodInvoker(Sub()
                                        btnQuery.PerformClick()
                                    End Sub))
        If _bRange = True Then
            Me.Invoke(New MethodInvoker(Sub()
                                            btnRange.PerformClick()
                                        End Sub))
        End If

    End Sub

    Private Sub dtpSt_ValueChanged(sender As Object, e As EventArgs) Handles dtpSt.ValueChanged
        If dtpEd.Value <= dtpSt.Value Then
            dtpEd.Value = DateAdd(DateInterval.Minute, 2, dtpSt.Value)
        End If
    End Sub

    Private Sub fn_MakeAnnotation(ByVal index As Integer)
        Dim stVerticalAnnotation As DataVisualization.Charting.VerticalLineAnnotation = chtCalls.MainChart.Annotations(0)
        Dim edVerticalAnnotation As DataVisualization.Charting.VerticalLineAnnotation = chtCalls.MainChart.Annotations(1)
        Dim stRectAnnotation As DataVisualization.Charting.RectangleAnnotation = chtCalls.MainChart.Annotations(2)
        Dim edRectAnnotation As DataVisualization.Charting.RectangleAnnotation = chtCalls.MainChart.Annotations(3)

        stVerticalAnnotation.AxisX = chtCalls.MainChart.ChartAreas(index).AxisX
        stVerticalAnnotation.AxisY = chtCalls.MainChart.ChartAreas(index).AxisY
        stVerticalAnnotation.X = chtCalls.GetMinimumAxisXChartArea(index)
        stVerticalAnnotation.Visible = True
        stVerticalAnnotation.AllowMoving = True
        stVerticalAnnotation.IsInfinitive = True
        stVerticalAnnotation.Name = "StartTime"
        stVerticalAnnotation.AnchorY = 200
        stVerticalAnnotation.ToolTip = "StartTime"
        stVerticalAnnotation.AllowTextEditing = True

        edVerticalAnnotation.AxisX = chtCalls.MainChart.ChartAreas(index).AxisX
        edVerticalAnnotation.AxisY = chtCalls.MainChart.ChartAreas(index).AxisY
        edVerticalAnnotation.X = chtCalls.GetMaximumAxisXChartArea(index)
        edVerticalAnnotation.Visible = True
        edVerticalAnnotation.AllowMoving = True
        edVerticalAnnotation.IsInfinitive = True
        edVerticalAnnotation.Name = "EndTime"
        edVerticalAnnotation.AnchorY = 200
        edVerticalAnnotation.ToolTip = "EndTime"

        AddHandler chtCalls.MainChart.AnnotationPositionChanging, AddressOf chtCPU_AnnotationPositionChanging
        AddHandler chtCalls.MainChart.AnnotationPositionChanged, AddressOf chtCPU_AnnotationPositionChanged

        btnRange.Text = p_clsMsgData.fn_GetData("F269", "On")
        btnRange.ForeColor = Color.Lime
    End Sub

    Private Sub fn_DeleteAnnotaion()
        RemoveHandler chtCalls.MainChart.AnnotationPositionChanging, AddressOf chtCPU_AnnotationPositionChanging
        RemoveHandler chtCalls.MainChart.AnnotationPositionChanged, AddressOf chtCPU_AnnotationPositionChanged

        chtCalls.MainChart.Annotations(0).Visible = False
        chtCalls.MainChart.Annotations(1).Visible = False

        For Each tmpChartArea As DataVisualization.Charting.ChartArea In chtCalls.MainChart.ChartAreas
            If tmpChartArea.Visible = True Then
                tmpChartArea.CursorX.SetSelectionPosition(-1, -1)
            End If
        Next

        btnRange.Text = p_clsMsgData.fn_GetData("F269", "Off")
        btnRange.ForeColor = Color.LightGray
        _bRange = False
    End Sub


    Private Sub frmStatements_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If _chtCount > 0 Then
            For i As Integer = 1 To _AreaCount
                chtCalls.SetInnerPlotXPositionChartArea(i, _chtCount)
                chtCalls.MainChart.ChartAreas(i).RecalculateAxesScale()
            Next
        End If
    End Sub

    Private Sub SetTopQueryIDs(ByVal index As Integer)
        Dim dtTable As DataTable = Nothing
        dtTable = _clsQuery.SelectStatementsTop(_InstanceID, dtpSt.Value, dtpEd.Value, index, cmbTop.Items(cmbTop.SelectedIndex))
        _arrQueryIDs.Clear()
        _arrDBIDs.Clear()
        If dtTable IsNot Nothing Then
            For i As Integer = 0 To dtTable.Rows.Count - 1
                _arrQueryIDs.Add(dtTable.Rows(i).Item("queryid"))
                _arrDBIDs.Add(dtTable.Rows(i).Item("dbid"))
            Next
        End If
    End Sub

    Private Sub cmbDuration_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim cbo As BaseControls.ComboBox = DirectCast(sender, BaseControls.ComboBox)
        Dim tempDt As Date = Now
        Select Case cbo.SelectedIndex
            Case 0
                tempDt = dtpSt.Value.AddMinutes(60)
            Case 1
                tempDt = dtpSt.Value.AddMinutes(120)
            Case 2
                tempDt = dtpSt.Value.AddMinutes(240)
            Case 3
                tempDt = dtpSt.Value.AddMinutes(720)
        End Select

        If tempDt > Now Then
            tempDt = Now
        End If
        dtpEd.Value = tempDt
    End Sub

    Private Sub rb1H_CheckedChanged(sender As Object, e As EventArgs) Handles rb1H.CheckedChanged, rb2H.CheckedChanged, rb4H.CheckedChanged, rb12H.CheckedChanged, rb1D.CheckedChanged
        'Dim Rb As BaseControls.RadioButton = DirectCast(sender, BaseControls.RadioButton)
        'If Rb.Checked = True Then
        '    dtpDay.Checked = False
        '    dtpEd.Value = DateTime.Now
        '    If Rb.Text.Equals("~1H") Then
        '        dtpSt.Value = dtpEd.Value.AddHours(-1)
        '    ElseIf Rb.Text.Equals("~2H") Then
        '        dtpSt.Value = dtpEd.Value.AddHours(-2)
        '    ElseIf Rb.Text.Equals("~4H") Then
        '        dtpSt.Value = dtpEd.Value.AddHours(-4)
        '    ElseIf Rb.Text.Equals("~12H") Then
        '        dtpSt.Value = dtpEd.Value.AddHours(-12)
        '    ElseIf Rb.Text.Equals("~1D") Then
        '        dtpSt.Value = dtpEd.Value.AddHours(-24)
        '    End If
        'End If
    End Sub


    Private Sub rb1H_Click(sender As Object, e As EventArgs) Handles rb1H.Click, rb2H.Click, rb4H.Click, rb12H.Click, rb1D.Click
        Dim Rb As BaseControls.RadioButton = DirectCast(sender, BaseControls.RadioButton)
        If Rb.Checked = True Then
            dtpDay.Checked = False
            dtpEd.Value = DateTime.Now
            If Rb.Text.Equals("~1H") Then
                dtpSt.Value = dtpEd.Value.AddHours(-1)
            ElseIf Rb.Text.Equals("~2H") Then
                dtpSt.Value = dtpEd.Value.AddHours(-2)
            ElseIf Rb.Text.Equals("~4H") Then
                dtpSt.Value = dtpEd.Value.AddHours(-4)
            ElseIf Rb.Text.Equals("~12H") Then
                dtpSt.Value = dtpEd.Value.AddHours(-12)
            ElseIf Rb.Text.Equals("~1D") Then
                dtpSt.Value = dtpEd.Value.AddHours(-24)
            End If
        End If
        btnQuery.PerformClick()
    End Sub

    Private Sub dtpDay_ValueChanged(sender As Object, e As EventArgs) Handles dtpDay.ValueChanged
        If dtpDay.Checked = True Then
            dtpSt.Value = New Date(dtpDay.Value.Year, dtpDay.Value.Month, dtpDay.Value.Day, 0, 0, 0, 0)
            dtpEd.Value = dtpSt.Value.AddHours(24)

            rb1H.Checked = False
            rb2H.Checked = False
            rb4H.Checked = False
            rb12H.Checked = False
            rb1D.Checked = False
            'btnQuery.PerformClick()
        End If
    End Sub

    Private Sub cmbSort_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSort.SelectedIndexChanged
        Dim cbo As BaseControls.ComboBox = DirectCast(sender, BaseControls.ComboBox)
        'Select Case cbo.SelectedIndex
        '    Case 0
        '        dgvStmtList.Sort(coldgvStmtCalls, System.ComponentModel.ListSortDirection.Descending)
        '    Case 1
        '        dgvStmtList.Sort(coldgvStmtTotalTime, System.ComponentModel.ListSortDirection.Descending)
        '    Case 2
        '        dgvStmtList.Sort(coldgvStmtCPUTimeRate, System.ComponentModel.ListSortDirection.Descending)
        '    Case 3
        '        dgvStmtList.Sort(coldgvStmtIOTimeRate, System.ComponentModel.ListSortDirection.Descending)
        'End Select
    End Sub

    Private Sub txtQueryID_TextChanged(sender As Object, e As EventArgs) Handles txtQueryID.TextChanged
        Try
            Dim rowFilter As String = String.Format("Convert([{0}], System.String) LIKE '%{1}%'", coldgvStmtQueryID.HeaderText, txtQueryID.Text)
            Dim dt As DataTable
            dt = dgvStmtList.DataSource.DataSource
            dt.DefaultView.RowFilter = rowFilter
        Catch ex As Exception
            GC.Collect()
        End Try
    End Sub

    Private Sub txtSQL_TextChanged(sender As Object, e As EventArgs) Handles txtSQL.TextChanged
        Try
            Dim rowFilter As String = String.Format("Convert([{0}], System.String) LIKE '%{1}%'", coldgvStmtQuery.HeaderText, txtSQL.Text)
            Dim dt As DataTable
            dt = dgvStmtList.DataSource.DataSource
            dt.DefaultView.RowFilter = rowFilter
        Catch ex As Exception
            GC.Collect()
        End Try
    End Sub

    Private Sub cbxHideSysSQL_CheckedChanged(sender As Object, e As EventArgs) Handles cbxHideSysSQL.CheckedChanged
        Dim rbTemp As BaseControls.CheckBox = DirectCast(sender, BaseControls.CheckBox)
        SetRowfilter(rbTemp)
    End Sub

    Private Sub btnEditFiltering_Click(sender As Object, e As EventArgs) Handles btnEditFiltering.Click
        Dim frmCS As New frmStatementsFilter(_ListStatements)
        If frmCS.ShowDialog = Windows.Forms.DialogResult.OK Then
            _ListStatements = frmCS.StatementList
            Dim clsIni As New Common.IniFile(p_AppConfigIni)
            Dim StatementFilters As String = ""
            For Each StatementFilter In _ListStatements
                StatementFilters = String.Join(",", StatementFilter)
            Next
            clsIni.WriteValue("STATSTATEMENTS", "StatementFilters", String.Join(",", _ListStatements))
            SetRowfilter(cbxHideSysSQL)
        End If
        frmCS.Dispose()
    End Sub

    Private Sub ReadConfig()
        Dim clsIni As New Common.IniFile(p_AppConfigIni)
        _UseFilter = clsIni.ReadValue("STATSTATEMENTS", "UseFilter", False)
        
        Dim StatementFilters As String() = clsIni.ReadValue("STATSTATEMENTS", "StatementFilters", "pg_catalog").Split(New Char() {","c})

        Dim StatementFilter As String
        For Each StatementFilter In StatementFilters
            If Not StatementFilter.Equals("") Then
                _ListStatements.Add(StatementFilter)
            End If
        Next
    End Sub
    Private Sub SetRowfilter(ByRef checkBox As BaseControls.CheckBox)
        Try
            Dim dt As DataTable
            If checkBox.Checked = True Then
                'Dim rowFilter As String = String.Format("Convert([{0}], System.String) NOT LIKE '%{1}%'", coldgvStmtQuery.HeaderText, "application_name")
                Dim rowFilter As String = ""
                Dim rowFilterList As String = ""
                For Each StatementFilter In _ListStatements
                    rowFilterList += String.Format("AND Convert([{0}], System.String) NOT LIKE '%{1}%' ", coldgvStmtQuery.HeaderText, StatementFilter)
                Next
                rowFilter = String.Format("Convert([{0}], System.String) <> '----' {1}", coldgvStmtQuery.HeaderText, rowFilterList)
                dt = dgvStmtList.DataSource.DataSource
                dt.DefaultView.RowFilter = rowFilter
                btnEditFiltering.Visible = True
            Else
                dt = dgvStmtList.DataSource
                dt.DefaultView.RowFilter = Nothing
                btnEditFiltering.Visible = False
                lslSession.Text = p_clsMsgData.fn_GetData("F323", dt.DefaultView.Count)
            End If

            lslSession.Text = p_clsMsgData.fn_GetData("F323", dt.DefaultView.Count)

            Dim clsIni As New Common.IniFile(p_AppConfigIni)
            clsIni.WriteValue("STATSTATEMENTS", "UseFilter", True)

        Catch ex As Exception
            GC.Collect()
        End Try

    End Sub

    ' Displays the drop-down list when the user presses 
    ' ALT+DOWN ARROW or ALT+UP ARROW.
    Private Sub dgvStmtList_KeyDown(ByVal sender As Object, _
        ByVal e As KeyEventArgs) Handles dgvStmtList.KeyDown

        If e.Alt AndAlso (e.KeyCode = Keys.Down OrElse e.KeyCode = Keys.Up) Then

            Dim filterCell As DataGridViewAutoFilterColumnHeaderCell = _
                TryCast(dgvStmtList.CurrentCell.OwningColumn.HeaderCell,  _
                DataGridViewAutoFilterColumnHeaderCell)
            If filterCell IsNot Nothing Then
                filterCell.ShowDropDownList()
                e.Handled = True
            End If

        End If

    End Sub

End Class
