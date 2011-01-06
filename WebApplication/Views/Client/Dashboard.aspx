<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="Microsoft.Web.Mvc" %>
<%@ Import Namespace="Br.Com.Quavio.Tools.Web.Mvc" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Dashboard</title>
	<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="Content-Language" content="pt-br" />
    <meta name="robots" content="all" />
    <meta name="description" content="" />
    <meta name="keywords" content="" />
	<%= Html.Css("~/Content/Reset.css")%>
	<%= Html.Css("~/Content/textformat.css")%>
	<%= Html.Css("~/Content/Site.css")%>
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.3/jquery.min.js" type="text/javascript"></script>
	<%= Html.Script("~/Scripts/util.js")%>
	<%= Html.Script("~/Scripts/Dashboard.js")%>
</head>
<body>
<% if (Model == null ||
	Model.TheClient == null) { %>
	<h1>Cliente não encontrado.</h1>
<% }
   else { %>
	<script>
		var dashboard = new Dashboard({
			clientId: '<%= Model.TheClient.Id %>',
			clientOnline: '<%= Model.TheClient.IsOnline %>',
			onlineUrl: '<%= Url.Action("Online", "Client") %>',
			chatUrl: '<%= Url.Action("HostStart", "Room", new { id = Model.TheClient.Id }) %>',
			queueUrl: '<%= Url.Action("Queue", "Client") %>'
		});
	</script>
	<h1 class="page-title">Atendimento Online</h1>
	<div class="dashboard">
		<%= Html.Image("~/Content/Images/Atendimento/" + (string)Model.TheClient.Logo, new { alt = "Logo", Class = "logo" })%>
		<h1><%= Model.TheClient.Name%></h1>
		<input type="button" value="Iniciar Atendimento" id="btnState" class="btnStart" />
		<div class="fila">
			<h2>Fila de atendimento: </h2>
			<table cellpadding="0" cellspacing="0" id="table-queue">
				<thead>
					<tr>
						<td>Nome</td>
						<td>Email</td>
					</tr>
				</thead>
				<tbody>
				<%	if(Model.TheClient.IsOnline) {
						foreach (var clnt in Model.TheClient.WaitingMembers) { %>
						<tr roomName="<%= clnt.RoomName %>">
							<td><%= clnt.Name %></td>
							<td><%= clnt.Email %></td>
						</tr>
					<%	}
				} %>
				</tbody>
			</table>
		</div>
	</div>
<%	} %>
</body>
</html>
