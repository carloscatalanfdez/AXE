#include "PolycodeView.h"
#include "Polycode.h"

using namespace Polycode;

class Game {
public:
    Game(PolycodeView *view);
    ~Game();
    
    bool Update();
    
private:
    Core *core;
};