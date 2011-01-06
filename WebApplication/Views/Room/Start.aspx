<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Com.CloudTalk.Webapp.Models.ChatViewModel>" %>
<%@ Import Namespace="Microsoft.Web.Mvc" %>
<%@ Import Namespace="Br.Com.Quavio.Tools.Web.MVC" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Atendimento Online</title>
    
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="Content-Language" content="pt-br" />
    <meta name="robots" content="all" />
    <meta name="description" content="" />
    <meta name="keywords" content="" />
    
    <%--<%= Html.FavIcon("~/Content/Images/Global/favicon.ico")%>--%>
    
    <%= Html.Css("~/Content/Reset.css")%>
    <%= Html.Css("~/Content/Common.css")%>
    <%= Html.Css("~/Content/textformat.css")%>
    <%= Html.Css("~/Content/Atendimento.css")%>
    <%= Html.Css("~/Content/stickyfooter.css")%>
    
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.3/jquery.min.js" type="text/javascript"></script>
	
	<%= Html.Script("~/Scripts/util.js")%>
    <%= Html.Script("~/Scripts/jquery.sound.js")%>
    <%= Html.Script("~/Scripts/date.format.min.js")%>
    <%= Html.Script("~/Scripts/1.0.0/cloudtalk.js")%>
    <%= Html.Script("~/Scripts/1.0.0/cloudtalk.ui.js")%>

    <script type="text/javascript">
        var cloudTalkUI = new CloudTalkUI({
			isOnline: '<%= Model.Online %>',
			isHost: '<%= Model.IsHost %>',
			clientId: '<%= ViewContext.RouteData.Values["id"].ToString() %>',
			isInvalid: '<%= Model.InvalidSession %>',
			baseUrl: '<%= Url.Content("~/Room.aspx/") %>',
			tunePath: '<%= Url.Content("~/Content/newmsg.wma") %>',
			autoStart: '<%= Model.AutoStart %>',
			userName: '<%= Model.Name %>',
			userEmail: '<%= Model.Email %>',
			roomName: '<%= Model.RoomName %>'
		});
    </script>
</head>
<body>
    <div id="wrap">
	    <div id="main" class="clearfix">
            <div class="chat-body">
                <div class="title">Atendimento Online <span class="float-right"><a href="#" id="btnSair">sair</a></span></div>
            <%	if (Model.Logo != null) { %>
				<%= Html.Image("~/Content/Images/Atendimento/" + Model.Logo)%>
			<%	} %>
                <div class="not-available">No momento, não existem posições disponíveis para seu atendimento. Por favor, tente novamente mais tarde.</div>
                <div class="please-wait">Por favor, aguarde. Você será atendido em alguns instantes.</div>
                <div class="thanks">Você foi desconectado. <br /><br />Obrigado por utilizar nosso atendimento online!</div>
                <span id="queue-indicator">Você está na posição <span id="queue-size">?</span> da fila. Por favor, aguarde.</span>
                <div id="login-panel">
                    Bem vindo ao nosso atendimento via chat!<br /><br />
                    <%= Html.Label("Nome:")%><%= Html.TextBox("Name", null, new { Class = "login-input", Id = "txt-name" })%><br />
                    <%= Html.Label("Email:")%><%= Html.TextBox("Email", null, new { Class = "login-input", Id = "txt-email" })%><br />
                    <%= Html.SubmitButton("Entrar", "Entrar", new { Class = "enter-button", Id = "enter-button" })%>
                    <%= Html.Image(Url.Content("~/Content/Images/Atendimento/loading.gif"), new { Class = "loading-image" })%>
                </div>
                <div id="messages-panel">
                    <div class="messages-area" id="msgs-panel">
                    </div>
                    <%= Html.TextArea("Message", new { Class = "input-area", Id = "input-area" })%>
                    <%= Html.SubmitImage("Enviar", "~/Content/Images/Atendimento/enviar.png", new { Class = "submit-button", Id = "submit-button" })%>
                </div>
            </div>
        </div>
    </div>
    <div class="chat-footer" id="footer">
        <a href="http://www.quavio.com.br/" target="_blank"><%= Html.Image("~/Content/Images/quavio.png", new { Class = "float-right" })%></a>            
    </div>
</body>
</html>
