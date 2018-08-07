# Movie-Analyzer
This project groups actors that are related to each other

The main idea behind this project is utilizing graph data structure. Nodes denote the actors. Two actors are connected with an edge, if they have played in the same movie. The edge's data is the genre of the movie. 

In order to do the clustering, I have implemented the min-cut algorithm. This is used for dividing the graph into two, more related subgraphs. This subroutine continues until all the subgraphs have sizes less than a specified number. All the groups are saved as json files in the specified directory.
