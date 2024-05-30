# How to test an engine for ELO gain

This guide is made for those developing a chess engine following Sebastian Lague's chess engine challenge https://github.com/SebLague/Chess-Challenge but most of this applies to all engines.

## TLDR
TLDR: Use UCI (I have an implementation here: https://github.com/GediminasMasaitis/Chess-Challenge-Uci/tree/uci), use CuteChess https://github.com/cutechess/cutechess CLI to run matches with concurrency = your physical core count, get an opening book from https://github.com/official-stockfish/books, I (and a lot of people) use UHO_XXL_+0.80_+1.09.epd, remember to set -games 2 -repeat when running the CLI

## Detailed procedure

### 1. Setting up UCI

The vast majority of chess engines use UCI, Universal Chess Protocol. The full specification can be found at https://wbec-ridderkerk.nl/html/UCIProtocol.html

There is an UCI implementation ready-to-go at https://github.com/GediminasMasaitis/Chess-Challenge-Uci/tree/uci, you would paste your code into the main project's MyBot.cs, but instead of building the main project as usual, build the project inside the`Chess-Challenge.Uci` directory. If you run it you should see something like:

```
Sebastian Lague's Chess Challenge UCI interface by Gediminas Masaitis
Current token count: 31 (31 total, 0 debug)
```

In the console, to verify, type `go `, it will print the move when it is given 60 seconds of total match time.

When building, make sure that the build configuration is set to Release (as opposed to of Debug).

### 2. Opening books

For proper testing, you will need an opening book. The reason for this is because if you test from the start position all the time, the engines will likely play the same opening most of the times, and you want to test a variety of openings, because you don't know what the opponent may do.

I personally use `UHO_XXL_+1.00_+1.29.epd` from https://github.com/official-stockfish/books, as do many top chess engine developers. You may sometimes hear this referred to as the "Pohl book", because it's the most used book submitted by Stefan Pohl.

UHO stands for "Unbalanced human openings". +1.00_+1.29 in the name refers to score difference between white and black, white always being on top, as evaluated by Stockfish. Why would you use such an imbalanced book? Well, it helps to reduce draw count. As a general rule, as the stronger your engine (and the opoonent) is, the more likely it is to draw games. Unbalanced books help reduce the draw ratio, and in doing so reduce the games needed to test ELO.

### 3. Getting CuteChess

Got to https://github.com/cutechess/cutechess/releases and download the latest release for your apropriate OS, install if needed. We will be only be using the CLI (command line interface) for this tutorial, but if you want to observe games versus other engines the GUI (graphical user interface) is pretty good, but I won't cover how to set it up.

Open a terminal, and navigate to the directory where CuteChess was installed/extracted, and run `.\cutechess-cli.exe -help`. This will print a list of parameters that can be used. Feel free to look through them, you may find something fun you want to try for your engine.

### 4. Testing theory

The main idea of testing chess engine improvements is self-testing, which means testing agains your previous version. You need to have a compiled executable of the current best version, as well as a compiled executable of the version you wish to test.

### 5. Doing the ELO test

You need to run `cutechess-cli` with specific parameters to start a test. CuteChess expects at least 2 engines to test.

Start the CuteChess GUI, go to Tools > Settings > Engines, press the plus icon, and add both your current main engine and the engine that has the experimental changes. Name it as you please, but keep the names in mind.

I recommend making a script in the CuteChess directory that you can run repeatedly, without the need to run the very many options.
Here is an example of my script for Windows:

```
cutechess-cli.exe ^
-engine conf=ChessChallenge ^
-engine conf=ChessChallengeDev ^
-each tc=0/5+0.05 ^
-maxmoves 1000 ^
-games 2 ^
-repeat ^
-resultformat wide2 ^
-ratinginterval 10 ^
-rounds 50000 ^
-concurrency 12 ^
-tournament gauntlet ^
-pgnout out.pgn ^
-openings file="C:\\Chess\\Suites\\UHO_XXL_+0.90_+1.19.epd" format=epd ^
```

Refer to `cutechess-cli.exe --help` for info about each parameter.

Linux users will probably know how to translate this to their own format.

**Key things to keep in mind**:
* Set `concurrency` to the number of physical cores you have, not the number of threads you have.
* Always set `-games 2` and `-repeat`, this will ensure that each position in the opening book gets played twice, with each engine switching black/white.

Open a terminal and navigate to where CuteChess is installed. Run the script. You will see the output of game result.

### 6. Time control

Although the current challenge is announced to be 60+0, I think it's still fine to test your engine on time controls such as 5+0.05 (which means 5 seconds per match  + 0.05 second increment). The "industry standard" is to have increment 100x less than the current starting time. If you wish to test the actual time controls that will be used in the tournament, set `tc=0/60+0`.