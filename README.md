My solution to the Cimpress challenge 2015
====================================
###Installation and Building
To build the code, open the solution in "Visual Studio Express 2013 for Windows Desktop" (free) or above and press <kbd>F6</kbd>. To run, press <kbd>F5</kbd>

### Introduction
My solution to the Cimpress challenge is based on the standard binary program formulation for a set partitioning problem. The given Cimpress Challenge problem is a special case of the set partitioning problem, due to the fact that for every valid combination of sets X (or squares in Cimpress terminology), that does not cover the universe U (or valid cells in the grid in Cimpress terminology) there is always a combination of other sets X', which can be used to cover the rest of U, so that X union X' equals U. This property makes it very easy to construct solutions, which I am exploiting in my solution.

### Approach
Solving the binary program with a standard (open source) solver yields the optimal solution. However, depending on the given instance, the price paid for optimality is solution time. Since there is a hard penalty for submitting solutions after 10 seconds, this price usually is too high. Some instances on the other hand are so easy to solve (generally smaller grids and/or grids with a high percentage of obstacles, both leading to fewer possible squares) that optimality can be achieved within 10 seconds. A quick analysis showed, that the number of possible squares is a relatively good proxy for the runtime. Consquently I implemented the following strategy:
1) Always start an optimization run with the complete binary program, yielding the optimal solution, hopefully within 10 seconds.
2) Start more optimization runs where a subset of squares is already fixed, thus eliminating a number of other squares that would conflict with the fixed squares. The rest of the free squares then gets chosen by a standard solver. By greedily choosing large squares in the fixation step, I am ensuring that a) this doesn't introduce a large penalty to the score and b) it eliminates a large number of other squares. Of course, this approach might also cut off optimality, but as mentioned in the introduction, that is a price one needs to pay given the short time.

1) and 2) are both started in parallel, whith 2) being started multiple times with different sets of fixed variables. If the globally optimal solution is found, the run is stopped and the result is reported. Otherwise the code runs for at least 10 seconds. If no solution was found it continues to run until the first solution is found. If one or many solutions are found within the first 10 seconds, the best solution is reported after the 10 seconds are over.

#### Libraries used
- [Coin Cbc](https://projects.coin-or.org/Cbc)
- [Json.NET](http://www.newtonsoft.com/json)
- [Microsoft Bcl Build components](http://blogs.msdn.com/b/bclteam/p/bclbuild.aspx)
- [Microsoft BCL portability pack](http://blogs.msdn.com/b/bclteam/p/bclbuild.aspx)
- [Microsoft HTTP Client Libaries](http://blogs.msdn.com/b/bclteam/p/httpclient.aspx) 

#### Comments
#####Parameter Tuning
There are a lot of parameters with the approach which influence the solution quality as well as the runtime. Solution quality and runtime are influenced by the number of free variables after fixation. The runtime is influenced by a lot of parameters the standard solver exposes, such as which heuristics to try when, which cuts to generate when, which cholesky factorization method to use and so on. By using parameter tuning tools such as [SMAC](http://www.cs.ubc.ca/labs/beta/Projects/SMAC/), these parameters can be tuned and thus further reduce the runtime and/or improve solution quality. A few experiments did not show significant improvement possibilities here. However these experiments can and should be run for a longer time to get a conclusive result.

#####Tie Breaking
There are quite a few situations where two solutions look the same to a solver in terms of objective value. Imagine a grid made up of 2x3 cells. One optimal solution is to put a 2x2 square at the [0,0] coordinate and fill the remaining two cells with squares of size 1, yielding a solution of 3. However another equivalent solution exists, where the larger square simply starts at coordinates [0,1]. In order to help a solver decide which of these two is the "better" solution without loosing optimality, one can introduce an artificial ordering of the squares which is reflected in the objective function. This technique is often used to tie-break situations like the one described, however for this particular set partitioning problem, I didn't measure any runtime improvements, which is why I deactivated this part of the code. It is still in there for reference [Solver.cs, Lines 125 and 135].

#####Variability
A quick variability analysis showed that performance variability is big in these models in combination with Cbc. To show performance variability issues, I randomly permutated the variables in a model and solved the permuted model. The runtime varied by a factor of up to 4. However exploiting performance variablity in this case did not work, because the standard ordering of the variables in my code seemed to always yield the best runtime. This behavior is highly unusual and could be investigated further. For this challenge the important take away from this experiment is that one does not need to parallelize the runs of permuted models.