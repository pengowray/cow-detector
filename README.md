![image](https://github.com/pengowray/cow-detector/assets/800133/ffd06b97-fce2-4277-8189-ed80bf94f671)

# cow-detector
Finds cow openings in chess.com tournaments and gives stats on usage.

Runs successfully as a quick script to get some stats and as a proof of concept. To be generally useful would need to be made more robust and generalized. Issues include: tournament id is hardcoded; JSON and PGN parsing works with chess.com data but is not very robust; cannot easily be changed to detect other openings; cannot search usage by player; fixed, inflexible output; does not cache API calls; does not make use of existing libraries for API calls.
