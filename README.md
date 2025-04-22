
## Today's materials:
The code examples from the slideshow presentation slides can be found in today's repo's repo.

In fact, I like this repo so much I've decided to deprecate/archive fullstack2025. (Can still be read, but not forked)

The repo "template_onion" still reflects the proposed structure, but is very barebones compared to today's examples.

The codebase is a Weather Station app, but can easily be run without MQTT stuff (there's a conditional that which disables this if there is no broker host defined in appsettings.json).

It's deployed to Firebase + Fly io and can be accessed here: `https://easvweatherstation.web.app/` or simply clone this repo `https://github.com/uldahlalex/fs25_14_2`

## Topics
- Info about Exam + Q&A
- Deployment revisit (now with Fly io as an alternative to google cloud run)
- Connection manager revisit
- Controllers vs Event Handlers and websocket flow of communication revisit

## Slides
- Link: https://docs.google.com/presentation/d/1wpWTdBgHJMMXP8qrWFvUsixkQJybVKdwH9ce2EMMZLM/edit?usp=sharing

## Second round of individualized development help

Will be Wednesday next week. You can book here: `https://moodle.easv.dk/mod/scheduler/view.php?id=192595` (or simply scroll down here on the Moodle page)


## Today's Activity 1: Problem Description for Exam Project
**Task:** Meet with your group (1 person groups are also viable). Figure out what your exam project should be.
Here are my general guidelines:
- Since you are free to pick any project, you should prioritize a project which benefits from the technologies used (Websockets and onion architecture must be used).
- Pick something where a "minimum viable product" / walking skeleton / usable demo is doable with a 6 week scope or less.
    - If the minimum viable product is reached before the submission date, it is preferable if certain features can be added / polished.


**You don't need to submit your exam project description or groups, but it is important that you prepare a description.**

If you're unsure whether or not your idea is doable / viable / relevant, please ask me (and possibly also the other elective course teachers if it is a multi-subject product).

## Today's Activity 2: Proficiency in the required tools

The only requirements for Fullstack are onion and websockets. Therefore, assess your progress / proficiency. Make sure you can succesfully do the following things:

- Connect client (browser/postman/etc) to WebSocket API + send messages both ways
- Use the connection manager to subscribe clients to topics and broadcast to topics
- Perform database CRUD operations in Onion with repository classes
- Client app can perform certain action when a message is broadcasted from API

Next Friday the final recap topic will be covered: Software Testing.

