Attribute VB_Name = "ApiConsumer"
Public Sub LoadProductsIntoListView(lst As ListView)
    Dim xhr As Object
    Set xhr = CreateObject("MSXML2.XMLHTTP.6.0")
    Dim url As String
    url = "http://localhost:5000/api/products"

    On Error GoTo ApiError
    xhr.Open "GET", url, False
    xhr.setRequestHeader "Accept", "application/json"
    xhr.Send

    If xhr.Status = 200 Then
        Dim script As Object
        Set script = CreateObject("MSScriptControl.ScriptControl")
        script.Language = "JScript"
        script.AddCode "function parseJSON(json){ return JSON.parse(json); }"

        Dim jsObj As Object
        Set jsObj = script.Run("parseJSON", xhr.responseText)

        lst.ListItems.Clear

        Dim i As Long
        For i = 0 To jsObj.length - 1
            Dim name As String
            Dim price As Variant
            name = jsObj(i).name
            price = jsObj(i).price
            Dim item As ListItem
            Set item = lst.ListItems.Add(, , name)
            item.SubItems(1) = Format(price, "0.00")
        Next i
    Else
        MsgBox "Erro ao consultar API: " & xhr.Status & " - " & xhr.statusText, vbExclamation
    End If

    Exit Sub

ApiError:
    MsgBox "Erro ao acessar API: " & Err.Description, vbCritical
End Sub
