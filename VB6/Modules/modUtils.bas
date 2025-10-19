Attribute VB_Name = "UIUtils"
Option Explicit

Public Sub SetupListView(ByVal lv As ListView, ByVal endpoint As ApiEndpoint)
    Dim cols As Object
    Dim colKey As Variant
    Dim colWidth As Long
    Dim index As Integer
    
    Set cols = GetListViewColumns(endpoint)
    
    With lv
        .View = lvwReport
        .ColumnHeaders.Clear
        
        index = 1
        For Each colKey In cols.Keys
            Select Case index
                Case 1: colWidth = 2500
                Case 2: colWidth = 1250
                Case Else: colWidth = 1500
            End Select
            
            .ColumnHeaders.Add , , colKey, colWidth
            index = index + 1
        Next colKey
    End With
End Sub

Public Sub AddListViewItem(ByVal lv As ListView, ByVal obj As Object, _
    ByVal keyMain As String, ByVal keySub As String, ByVal formatType As String)
    On Error GoTo ErrorHandler
    
    Dim item As ListItem
    Dim valueMain As Variant
    Dim valueSub As Variant
    
    If obj.Exists(keyMain) Then valueMain = obj(keyMain)
    If keySub <> "" And obj.Exists(keySub) Then valueSub = obj(keySub)
    
    Set item = lv.ListItems.Add(, , FormatValue(valueMain, formatType))
    
    If keySub <> "" Then
        item.SubItems(1) = FormatValue(valueSub, formatType)
    End If
    
    Exit Sub

ErrorHandler:
    MsgBox "Erro ao adicionar itens do ListView: " & Err.Description, vbCritical, "Erro"
End Sub

Public Sub AddListViewRow(ByVal lv As ListView, ByVal obj As Object, ByVal cols As Object)
    On Error GoTo ErrorHandler
    Dim item As ListItem
    Dim colKey As Variant
    Dim cfg As Object
    Dim colIndex As Long
    
    colIndex = 0
    
    For Each colKey In cols.Keys
        Set cfg = cols(colKey)
        
        Dim value As Variant
        If cfg("keyMain") <> "" And obj.Exists(cfg("keyMain")) Then
            value = obj(cfg("keyMain"))
        Else
            value = ""
        End If
        
        value = FormatValue(value, cfg("formatType"))
        
        If colIndex = 0 Then
            Set item = lv.ListItems.Add(, , value)
        Else
            item.SubItems(colIndex) = value
        End If
        
        colIndex = colIndex + 1
    Next colKey
    
    Exit Sub

ErrorHandler:
    MsgBox "Erro ao adicionar o ListView: " & Err.Description, vbCritical, "Erro"
End Sub

Private Function CreateColumnConfig(ByVal keyMain As String, ByVal keySub As String, _
    Optional formatType As String = "text") As Object
    Dim cfg As Object
    Set cfg = CreateObject("Scripting.Dictionary")
    
    cfg.Add "keyMain", keyMain
    cfg.Add "keySub", keySub
    cfg.Add "formatType", formatType
    
    Set CreateColumnConfig = cfg
End Function

Public Function GetListViewColumns(ByVal endpoint As ApiEndpoint) As Object
    On Error GoTo ErrorHandler

    Dim cols As Object
    Set cols = CreateObject("Scripting.Dictionary")
    
    Select Case endpoint
        Case EndpointProdutos
            cols.Add "Produto", CreateColumnConfig("name", "", "text")
            cols.Add "Preço", CreateColumnConfig("price", "", "currency")
            cols.Add "Status", CreateColumnConfig("active", "", "boolean")
            cols.Add "Cadastro", CreateColumnConfig("createdAt", "", "date")
        Case EndpointClientes
            cols.Add "Cliente", CreateColumnConfig("name", "document", "text")
            cols.Add "Ativo", CreateColumnConfig("active", "", "boolean")
    End Select
    
    Set GetListViewColumns = cols

    Exit Function

ErrorHandler:
    MsgBox "Erro ao atualizar a ListView: " & Err.Description, vbCritical, "Erro"
End Function

Private Function FormatValue(ByVal value As Variant, ByVal formatType As String) As String
    On Error Resume Next
    
    Dim dtPart As String
    Select Case LCase(formatType)
        Case "currency"
            FormatValue = "R$ " & FormatNumber(CDbl(value), 2)
        Case "date"
            If Not IsEmpty(value) And Len(value) >= 10 Then
                dtPart = Left$(value, 10)
                dtPart = Replace$(dtPart, "-", "/")
                If IsDate(dtPart) Then
                    FormatValue = Format(CDate(dtPart), "dd/mm/yyyy")
                Else
                    FormatValue = value
                End If
            Else
                FormatValue = value
            End If
        Case "boolean"
            If CBool(value) Then
                FormatValue = "Ativo"
            Else
                FormatValue = "Inativo"
            End If
        Case "status"
            FormatValue = UCase$(value)
        Case "integer"
            FormatValue = FormatNumber(CInt(value))
        Case Else
            FormatValue = CStr(value)
    End Select
End Function
