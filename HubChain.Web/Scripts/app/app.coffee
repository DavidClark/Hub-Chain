# CoffeeScript
requirejs.config
	baseUrl: '../scripts'
	map:
		'*':
			ko:'knockout'
	shim:
		bootstrap:['jquery']
		breeze:['Q','jquery', 'knockout']
		sinonie:['sinon']
		sigr: ["jquery"]
		"flot.time": ["flot"]
		jqueryFileDownload:["jquery"]
	paths:
		sigr: "jquery.signalR-2.1.2"
		jquery:"jquery-2.1.3"
		knockout:"knockout-3.2.0"
		"knockout.punches":"knockout.punches.min"
		"linq":"linqjs-amd"

require [
	"jquery"
	"linq"
	"sigr"
	"knockout"
	"knockout.punches"
],($,linq,signalr,ko)->
	crossConnection = $.hubConnection "/signalr"
	crossProxy =  crossConnection.createHubProxy "newHub"
	#register a method to ensure onconnected + onDisconnected work on the server
	viewModel = 
		values:{}
		latest:ko.observableArray()
	count=0
	update=(message)->
		if viewModel.values[message.Id]
			viewModel.values[message.Id].value message.Value
			viewModel.values[message.Id].index message.Index
			viewModel.values[message.Id].exception message.Exception


	crossProxy.on "Update", (message)->
		update message

		#alert message
	starter = crossConnection.start()
	
	leave = (groups)->
		crossProxy.invoke("UnsubscribeItem", groups.map (g)->Id:g).then ()->
			for group in groups
				viewModel.latest.remove viewModel.values[group]
				delete viewModel.values[group]
	

	join = (groups)->
		for group in groups
			viewModel.values[group] or=
				(()->
					ret= 
						id:group
						value:ko.observable 'subscribing from client'
						index:ko.observable -999
						exception:ko.observable()
					viewModel.latest.push ret
					ret
				)()
		crossProxy.invoke("SubscribeItem", groups.map (g)->Id:g).then (data)->
			data.map (d)->update d
			
	toggleJoin=(joins,time)->
		setTimeout(->
			join(joins).then(->
				setTimeout(->
					toggleLeave joins, time
				Math.random() * time)
			).fail ->
				setTimeout(->
					toggleJoin joins, time
				1000)
		1000* Math.random())
	toggleLeave=(leaves, time)->
		leave(leaves).then(->
			setTimeout(->
				toggleJoin leaves, time
			Math.random() * time)
		).fail ->
			setTimeout(->
				toggleLeave leaves, time
			1000)
	starter.done ()->
		toggleJoin [1876], 5000
		toggleJoin [1801], 5000
		toggleJoin [1876], 20000
		toggleJoin [1855576], 20000
		toggleJoin ["anwothersss"], 5000
		toggleJoin [1,2,3,4,5,6,7,8,9,0,11,12,13,14,15,16,17,18,19,20], 5000
	
		

	$(document).ready ()->

##Bind view model to page and start pager js.

			ko.punches.enableAll()
			ko.applyBindings viewModel