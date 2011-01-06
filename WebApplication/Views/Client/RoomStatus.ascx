<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Com.CloudTalk.Data.ChatRoom>" %>
<%@ Import Namespace="Microsoft.Web.Mvc" %>
<%@ Import Namespace="Br.Com.Quavio.Tools.Web.Mvc" %>

<script>
	$(function () {
		$(".btnDelete").click(function () {
			$.ajax({
				type: 'POST',
				url: '<%= Url.Action("LeaveRoom", "Home") %>',
				data: {
					Email: $(this).attr("Email"),
					RoomName: '<%= Model.Name %>',
					Forced: true
				},
				dataType: "json"
			});

			$(this).parent().parent().remove();
		});
	});
</script>
<div class="status-page">
	<h3>Integrantes da sala</h3>
<%	if (Model.Members.Count() == 0) { %>
	<p>Nenhum cliente nesta sala</p>
<%	}
else { %>
	<table cellpadding="0" cellspacing="0">
		<thead>
			<tr>
				<td>Nome</td>
				<td>Email</td>
				<td>Remover</td>
			</tr>
		</thead>
		<tbody>
		<%	foreach (var item in Model.Members) { %>
			<tr>
				<td><%= item.Name%></td>
				<td><%= item.Email%></td>
				<td><%= Html.Image("~/Content/Images/cross.png", new { Class = "btnDelete", Email = item.Email })%></td>
			</tr>
		<%	} %>
		</tbody>
	</table>
<%	} %>
	<h3>Fila de espera</h3>
<%	if (Model.WaitingMembers.Count() == 0) { %>
	<p>Nenhum cliente na fila desta sala</p>
<%	}
else { %>
	<table cellpadding="0" cellspacing="0">
		<thead>
			<tr>
				<td>Nome</td>
				<td>Email</td>
				<td>Remover</td>
			</tr>
		</thead>
		<tbody>
		<%	foreach (var item in Model.WaitingMembers) { %>
			<tr>
				<td><%= item.Name%></td>
				<td><%= item.Email%></td>
				<td><%= Html.Image("~/Content/Images/cross.png", new { Class = "btnDelete", Email = item.Email })%></td>
			</tr>
		<%	} %>
		</tbody>
	</table>
<%	} %>
</div>