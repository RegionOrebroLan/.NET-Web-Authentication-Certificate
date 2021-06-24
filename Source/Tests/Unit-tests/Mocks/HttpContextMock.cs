using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;

namespace UnitTests.Mocks
{
	public class HttpContextMock : HttpContext
	{
		#region Properties

		[Obsolete("The base is obsolete.")]
		public override AuthenticationManager Authentication => this.InternalHttpContext.Authentication;

		public override ConnectionInfo Connection => this.InternalHttpContext.Connection;
		public override IFeatureCollection Features => this.InternalHttpContext.Features;
		protected virtual DefaultHttpContext InternalHttpContext { get; } = new DefaultHttpContext();
		protected virtual HttpRequest InternalHttpRequest { get; set; }
		protected virtual HttpResponse InternalHttpResponse { get; set; }

		[SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
		public override IDictionary<object, object> Items
		{
			get => this.InternalHttpContext.Items;
			set => this.InternalHttpContext.Items = value;
		}

		public override HttpRequest Request => this.InternalHttpRequest ??= new DefaultHttpRequest(this);

		public override CancellationToken RequestAborted
		{
			get => this.InternalHttpContext.RequestAborted;
			set => this.InternalHttpContext.RequestAborted = value;
		}

		public override IServiceProvider RequestServices
		{
			get => this.InternalHttpContext.RequestServices;
			set => this.InternalHttpContext.RequestServices = value;
		}

		public override HttpResponse Response => this.InternalHttpResponse ??= new DefaultHttpResponse(this) {Body = this.ResponseStream};
		public virtual StreamMock ResponseStream { get; } = new StreamMock();

		public override ISession Session
		{
			get => this.InternalHttpContext.Session;
			set => this.InternalHttpContext.Session = value;
		}

		public override string TraceIdentifier
		{
			get => this.InternalHttpContext.TraceIdentifier;
			set => this.InternalHttpContext.TraceIdentifier = value;
		}

		public override ClaimsPrincipal User
		{
			get => this.InternalHttpContext.User;
			set => this.InternalHttpContext.User = value;
		}

		public override WebSocketManager WebSockets => this.InternalHttpContext.WebSockets;

		#endregion

		#region Methods

		public override void Abort()
		{
			this.InternalHttpContext.Abort();
		}

		public virtual void SetRequest(HttpRequest request)
		{
			this.InternalHttpRequest = request;
		}

		public virtual void SetResponse(HttpResponse response)
		{
			this.InternalHttpResponse = response;
		}

		#endregion
	}
}