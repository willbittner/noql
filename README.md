# NoQL Streaming Complex Event Processor
NoQL is a Complex Event Processor designed to handle millions of realtime events per second in microsecond latency with either a drag and drop GUI or LINQ -like Syntax

It is capable of processing millions of events per second at microsecond latency on a laptop yet it abstracts away parallel and concurrent programming entirely!


# Quickstart:
NoQL is built with the concept of blocks and pipes and data flowing through them. You wire the blocks together with pipes and attach Input and Output adapters to the blocks.

Here are the current block types:
 - GroupBy
 - Select
 - Where
 - Window
 - Query 
 - Split
 - Save
 - NotIn
 - Sum
 - Count
 - Perform
 
A block can be user created and combined of a complex embedded network of blocks and pipes, or a user defined new kind of bock, or simply a block above

Code re-use is acheived by creating networks of blocks and pipes that are effective "functions" in a block and used later on just like a simple block regardless of what is underneath

