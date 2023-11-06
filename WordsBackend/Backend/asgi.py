from firebase import InitializeFirebase
from apscheduler.schedulers.background import BackgroundScheduler
import time
import cross_words
from datetime import datetime, timedelta
from firebase_admin import db


# clean up waiting rooms
def clean_waiting_rooms():
    db.reference().child("waitingRooms").delete()

scheduler = BackgroundScheduler()

program_running = True


TIME_INTERVAL = 2.0 # 2 minutes
ROOM_INTERVAL = 0.5 # 30 seconds 

def getNow():
    timestamp = int(time.time() * 1000)  # Get current Unix timestamp in milliseconds
    return timestamp


def end_game(roomId):
    db.reference().child("waitingRooms").child(roomId).update(
        {"status": "ended", "time": int(time.time())}
    )


def start_game(roomId):
    start = time.time()
    crosswordTableToSend, words = cross_words.gen_crossword_table()
    end = time.time()
    generateTime = end - start

    # response is JSON with table,words and time_took
    gameData = {
        "crosswordTable": crosswordTableToSend,
        "words": words,
        "generateTime": generateTime,
    }

    db.reference().child("waitingRooms").child(roomId).update(
        {"status": "started", "time": getNow(), "gameData": gameData}
    )

    delay = timedelta(
        minutes=TIME_INTERVAL + 0.05  # Delay of 2.05 minutes before end game
    )  # Delay of 2 minutes before end game
    run_time = datetime.now() + delay
    scheduler.add_job(end_game, "date", run_date=run_time, args=[roomId])


def close_waiting_room(roomId):
    try:
        waitingRoomRef = db.reference().child("waitingRooms").child(roomId)
        waitingRoom = waitingRoomRef.get()
        if (
            waitingRoom is None
            or "users" not in waitingRoom
            or len(waitingRoom["users"]) <= 0
        ):
            waitingRoomRef.delete()
        else:
            waitingRoomRef.update({"status": "closed", "time": getNow()})
            delay = timedelta(
                minutes=ROOM_INTERVAL
            )  # Delay of 0.5 minutes before start game
            run_time = datetime.now() + delay
            scheduler.add_job(start_game, "date", run_date=run_time, args=[roomId])
    except Exception as e:
        print(e)


def open_waiting_room():
    new_room = db.reference().child("waitingRooms").push()
    new_room.set(
        {"status": "open", "roomId": new_room.key, "time": getNow(), "users": []}
    )
    delay = timedelta(minutes=TIME_INTERVAL)  # Delay of 2 minutes
    run_time = datetime.now() + delay
    job = scheduler.add_job(
        close_waiting_room, "date", run_date=run_time, args=[new_room.key]
    )
    # check jobs
    print("Number of scheduled jobs:", len(scheduler.get_jobs()))
    print("Opened room " + new_room.key)
    print("Job scheduled at " + str(job.trigger.run_date))


def clean_closed_empty_rooms():
    rooms_ref = db.reference("waitingRooms")
    rooms = rooms_ref.get()
    if rooms is None:
        return

    for room_key, room_data in rooms.items():
        # make sure room_data is not empty object {}
        if not room_data:
            continue
        has_not_users = "users" not in room_data or len(room_data["users"]) == 0
        time_passed_2_minutes = (getNow() - room_data["time"]) > 120000

        if (
            ((room_data["status"] == "open") and time_passed_2_minutes)
            or has_not_users
            and (room_data["status"] == "started" or room_data["status"] == "closed")
            or room_data["status"] == "ended"
        ):
            db.reference(f"waitingRooms/{room_key}").delete()


def on_startup():
    # initialize firebase
    InitializeFirebase()
    # clear all waiting rooms
    clean_waiting_rooms()
    # add jobs to scheduler
    scheduler.add_job(open_waiting_room, "interval", minutes=TIME_INTERVAL)
    scheduler.add_job(clean_closed_empty_rooms, "interval", minutes=10.0)
    # delete all waiting rooms
    db.reference().child("waitingRooms").delete()
    # open first waiting room
    open_waiting_room()
    # start scheduler
    scheduler.start()
    # check if scheduler is running
    print("Scheduler running:", scheduler.running)
    # check number of scheduled jobs
    print("Number of scheduled jobs:", len(scheduler.get_jobs()))


# Start the thread
on_startup()

# Keep the program running
while program_running:
    try:
        time.sleep(60)  # sleep for 60 seconds
    except KeyboardInterrupt:
        program_running = False
        scheduler.shutdown()
