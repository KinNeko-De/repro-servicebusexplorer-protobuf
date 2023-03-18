# Motivation
Repro to reproduce and test if re-sending of message is supported by the service bus explorer in azure and github

In both cases i use protobuf to serialize a message to a byte array.
Then send it to the azure service bus.
Then receive it. It can be parsed back to the message.
I move it then to the dead letter queue.

Then you have to manually resend the dead letter queue.

The program controls if the message can be parsed or if the parsing fails.

Feature request created:
* [Azure](https://feedback.azure.com/d365community/idea/be0de061-d0a6-ed11-a81b-6045bd8615b0)
