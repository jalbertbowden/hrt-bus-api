from datetime import datetime, timedelta
from flask import Flask, render_template
from geopy import geocoders
import json
import os
import pymongo

app = Flask(__name__)
db = pymongo.Connection(os.environ['MONGO_URI']).hrt
dthandler = lambda obj: obj.isoformat() if isinstance(obj, datetime) else None

@app.route('/')
def hello():
    return 'Hello World!'

@app.route('/busfinder/')
def busFinder():
	return render_template('busfinder.html')

@app.route('/api/routes/active/')
def getActiveRoutes():
	activeRoutes = db['checkins'].distinct('routeId')
	activeRoutesWithDetails = db['routes'].find({'route_id': {'$in': activeRoutes}}, fields={'_id': False}).sort('route_id')
	return json.dumps(list(activeRoutesWithDetails))

@app.route('/api/buses/on_route/<int:routeId>/')
def getBusesOnRoute(routeId):
	checkins = {}
	for checkin in db['checkins'].find({'routeId':routeId, 'location': {'$exists': True}, 'adherence': {'$exists': True}}, fields={'_id': False, 'tripId': False}).sort('time'):
		checkins[checkin['busId']] = checkin
	return json.dumps(checkins.values(), default=dthandler)
	
@app.route('/api/buses/history/<int:busId>/')
def getBusHistory(busId):
	checkins = db['checkins'].find({'busId':busId, 'location': {'$exists': True}, 'adherence': {'$exists': True}}, fields={'_id': False, 'tripId': False}).sort('time', pymongo.DESCENDING)
	return json.dumps(list(checkins), default=dthandler)

@app.route('/api/stops/near/<city>/<intersection>/')
def getNearestStop(city, intersection):
	geocoders.Google()
	place, (lat, lng) = geocoders.Google().geocode("{0}, {1}, VA".format(intersection, city))
	return json.dumps(db['stops'].find_one({"location": {"$near": [lng, lat]}}))

@app.route('/api/stop_times/<int:routeId>/<int:stopId>/')
def getNextBus(routeId, stopId):
	time = datetime.utcnow()
	collectionName = 'gtfs_' + (time + timedelta(hours=-5)).strftime('%Y%m%d')
	scheduledStops = db[collectionName].find({"route_id":routeId, "stop_id":stopId})
	data = []
	for stop in scheduledStops:
		checkins = db['checkins'].find({"tripId":stop["trip_id"]}).sort('time', pymongo.DESCENDING)
		for checkin in checkins:
			try:
				stop['adherence'] = checkin['adherence']
				break
			except KeyError:
				pass
		data.append(stop)
	return json.dumps(data)

if __name__ == '__main__':
    # Bind to PORT if defined, otherwise default to 5000.
    port = int(os.environ.get('PORT', 5000))
    app.run(host='0.0.0.0', port=port, debug=True)
