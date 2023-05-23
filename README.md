![image](https://github.com/pengowray/cow-detector/assets/800133/ffd06b97-fce2-4277-8189-ed80bf94f671)

# cow-detector
Finds cow openings in PGNs and gives stats on usage.

Runs successfully as a quick script to get some stats and as a proof of concept. PGN parser isn't bad now but issues include: no CLI or UI to choose file or url; tournament/player id/url is hardcoded; fixed, inflexible output; does not cache API calls; does not make use of existing libraries for API calls; chess.com's API only gives data for last round of a tournament.
