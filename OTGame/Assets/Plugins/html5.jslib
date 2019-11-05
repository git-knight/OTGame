mergeInto(LibraryManager.library, {
  WebGL_Start: function () {
	  hubConnection.start().catch(function(err){console.log(err);});
  },

  WebGL_GetAuthToken: function() {
    var returnStr = localStorage.getItem('auth_token');
    var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
    stringToUTF8(returnStr, buffer, lengthBytesUTF8(returnStr) + 1);
    return buffer;
  },

  WebGL_Invoke: function(target, values) {
	  hubConnection.invoke.apply(hubConnection, [Pointer_stringify(target)].concat(JSON.parse(Pointer_stringify(values))));
  }
});