Attribute VB_Name = "Api"
Option Explicit

Public Enum ApiEndpoint
    EndpointProdutos = 1
    EndpointClientes = 2
End Enum

Private Const cJsonLib = 1
Private Const cBASE_URL As String = "https://localhost:51976/api/"

Public Function SearchApi(ByVal lv As ListView, ByVal endpoint As ApiEndpoint, _
    ByRef erro As String, Optional ByVal term As String = "") As Boolean

    Dim jsonData As Object
    Set jsonData = GetJsonFromApi(endpoint, term)

    If Not jsonData Is Nothing Then
        PopulateListViewFromJson lv, jsonData, endpoint
        SearchApi = True
        Exit Function
    Else
        erro = "Não foi possivel obter os dados da API."
        Exit Function
    End If
End Function

Public Function GetJsonFromApi(ByVal endpoint As ApiEndpoint, Optional ByVal param As String = "") As Object
    Dim http As Object
    Dim url As String
    Dim jsonResponse As Object
    Dim jsonRoot As Object

    Set http = CreateObject("MSXML2.XMLHTTP")

    Select Case endpoint
        Case EndpointProdutos
            url = cBASE_URL & "products/all"
        Case EndpointClientes
            If param <> "" Then
                url = cBASE_URL & "Customers/search?term=" & param
            Else
                url = cBASE_URL & "Customers/search"
            End If
    End Select

    On Error GoTo ApiError
    http.Open "GET", url, False
    http.setRequestHeader "Accept", "application/json"
    http.send

    If http.Status = 200 Then
        Set jsonResponse = JsonConverter.ParseJson(http.responseText)
        
        If jsonResponse.Exists("data") Then
            Set GetJsonFromApi = jsonResponse("data")
        Else
            Set GetJsonFromApi = jsonResponse
        End If
    Else
        HandleApiError http
    End If

    Exit Function

ApiError:
    MsgBox "Erro ao acessar API: " & Err.Description, vbCritical
End Function

Private Sub HandleApiError(ByVal http As Object)
    On Error GoTo Fallback

    Dim jsonError As Object
    Dim errMessage As String
    Dim item As Variant

    Set jsonError = JsonConverter.ParseJson(http.responseText)
    errMessage = ""

    If Not jsonError("errors") Is Nothing Then
        If TypeName(jsonError("errors")) = "Collection" And jsonError("errors").Count > 0 Then
            errMessage = "Erro de validação detectado:" & vbCrLf
            For Each item In jsonError("errors")
                errMessage = errMessage & "- " & CStr(item) & vbCrLf
            Next item
        End If
    End If

    If jsonError.Exists("message") Then
        If errMessage <> "" Then errMessage = errMessage & vbCrLf
        errMessage = errMessage & " " & jsonError("message")
    End If

    If errMessage = "" Then
        If jsonError.Exists("title") Then
            errMessage = " " & jsonError("title") & _
                         " (" & jsonError("status") & ")"
        Else
            errMessage = "Erro inesperado na resposta da API."
        End If
    End If

    MsgBox errMessage, vbExclamation, "Falha na API (" & http.Status & ")"
    Exit Sub

Fallback:
    MsgBox "Erro ao consultar API (" & http.Status & "): " & vbCrLf & _
           http.statusText & vbCrLf & vbCrLf & http.responseText, vbCritical
End Sub

Public Sub PopulateListViewFromJson(ByVal lv As ListView, ByVal data As Object, ByVal endpoint As ApiEndpoint)
    On Error GoTo ErrorHandler
    
    Dim itemsCollection As Object
    Dim obj As Object
    Dim cols As Object
    Dim colCfg As Object
    Dim key As Variant
    
    lv.ListItems.Clear
    
    Set itemsCollection = GetItemsCollection(data, endpoint)
    If itemsCollection Is Nothing Then Exit Sub
    
    Set cols = GetListViewColumns(endpoint)
    
    For Each obj In IterateCollection(itemsCollection)
        AddListViewRow lv, obj, cols
    Next obj
    
    Exit Sub

ErrorHandler:
    MsgBox "Erro ao popular o ListView: " & Err.Description, vbCritical, "Erro"
End Sub

Private Function GetItemsCollection(ByVal data As Object, ByVal endpoint As ApiEndpoint) As Object
    On Error GoTo ErrorHandler
    Dim collection As Object
    Dim innerData As Object
    If TypeName(data) = "Dictionary" Or TypeName(data) = "Scripting.Dictionary" Then
        Select Case endpoint
            Case EndpointProdutos
                If data.Exists("data") Then Set collection = data("data")
            Case EndpointClientes
                If data.Exists("data") Then
                    Set innerData = data("data")
                    
                    If TypeName(innerData) = "Dictionary" Or TypeName(innerData) = "Scripting.Dictionary" Then
                        If innerData.Exists("data") Then
                            Set collection = innerData("data")
                        Else
                            Set collection = innerData
                        End If
                    Else
                        Set collection = innerData
                    End If
                End If
        End Select
    ElseIf TypeName(data) = "Collection" Then
        Set collection = data
    Else
        MsgBox "Tipo de dado não suportado: " & TypeName(data), vbExclamation, "Erro"
        Set collection = Nothing
    End If
    
    Set GetItemsCollection = collection
    
    Exit Function

ErrorHandler:
    MsgBox "Erro ao alimentar itens: " & Err.Description, vbCritical, "Erro"
End Function

Private Function IterateCollection(ByVal items As Object) As collection
    On Error GoTo ErrorHandler
    Dim col As New collection
    Dim key As Variant
    
    Select Case TypeName(items)
        Case "Collection"
            Dim i As Long
            For i = 1 To items.Count
                col.Add items(i)
            Next i
        Case "Dictionary", "Scripting.Dictionary"
            For Each key In items.Keys
                col.Add items(key)
            Next key
    End Select
    
    Set IterateCollection = col
    
    Exit Function
ErrorHandler:
    MsgBox "Erro ao iterar na collection: " & Err.Description, vbCritical, "Erro"
End Function
