<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="Microsoft.Web.Mvc" %>
<%@ Import Namespace="Br.Com.Quavio.Tools.Web.Mvc" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Status das salas</title>
	<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="Content-Language" content="pt-br" />
    <meta name="robots" content="all" />
    <meta name="description" content="" />
    <meta name="keywords" content="" />
	<%= Html.Css("~/Content/Site.css")%>	
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.3/jquery.min.js" type="text/javascript"></script>
	<%= Html.Script("~/Scripts/util.js")%>
</head>
<body>
	<div class="status-page">
		<h1>Todas as salas:</h1>
		<ul>
	<%	foreach (var item in Model) { %>
			<li>
				<%= item.Name %>
				<% Html.RenderPartial("RoomStatus", (object)item); %>
			</li>
	<%	} %>
		</ul>
	</div>
</body>
</html>
