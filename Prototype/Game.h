#include "PolycodeView.h"
#include "Polycode.h"

using namespace Polycode;

class Game : public Win32Core {
public:
    Game(PolycodeView *view);
    ~Game();
    
    bool update();
};