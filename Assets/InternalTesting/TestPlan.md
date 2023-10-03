## Tester Details
**Tester Name**: [Your Name]  
**Chains Tested**: [List of chains]  
**Platforms Tested**: [List of platforms]  
**Wallets Used**: [List of wallets]

---

## Featureset 1 Persona System

### Feature 1.1 Dashboard Screen

This is the screen with the hexagon UI that shows the user's current persona and the personas they can switch to.

#### Unit 1.1.1: Basic functionality of the dashboard screen.

**Priority**: High

**Test Description**:
Basic functionality of the dashboard screen. Making sure that the UI is displayed correctly and that the user can view all the correct persona information.

**Glossary**
- Active Persona: The persona that the user has currently set to active in Emergence.
- Selected Persona: The persona that the user has selected to view in the dashboard screen, it may not be the same as the active persona.

**Required Inputs**:
- The user has at least four personas.

**Expected Output**:
- [ ] There is a hexagon for each persona.
- [ ] The current persona is highlighted by a green light in the top corner of that hexagon.
- [ ] No other persona has a green light.
- [ ] Clicking on a persona moves that one into the center, and its neighbours move to accomodate.
- [ ] The user can see the name and description of the current persona in the box below the icon.
- [ ] Clicking the right arrow moves the hexagons one space to the right.
- [ ] Clicking the left arrow moves the hexagons one space to the left.


#### Unit 1.1.2: Intermediate functionality of the dashboard screen.

**Priority**: Medium

**Test Description**:
Basic functionality of the dashboard screen. Making sure that the UI is displayed correctly and that the user can view all the correct persona information.

**Glossary**
- Active Persona: The persona that the user has currently set to active in Emergence.
- Selected Persona: The persona that the user has selected to view in the dashboard screen, it may not be the same as the active persona.

**Required Inputs**:
- The user has at least four personas.

**Expected Output**:
- [ ] When the UI is first opened the current selected persona is the active persona.
- You should only see 2 personas either side of the current persona (It is important to have at least 4 personas for this check):
  - [ ] If you select the rightmost hexagon you shouldn't see the leftmost hexagon.
  - [ ] If you select the leftmost hexagon you shouldn't see the rightmost hexagon.
- [ ] If you select the second hexagon from the right you should see the leftmost hexagon.
- [ ] If you select the second hexagon from the left you should see the rightmost hexagon.
- [ ] Clicking the small hexagon in the top left corner of the screen takes you to the current active persona.
- [ ] Clicking the small plus below the small hexagon takes you to the create persona screen.
- [ ] The active persona has a badge in the description box that says "Active"
- [ ] A selected non-active persona has a button to activate it in the description box.
- [ ] Pressing the paper and pen icon takes you to the edit persona screen.
- [ ] The trash icon is greyed out on the active persona.
- [ ] Pressing the trash on a non-active icon deletes the persona.
- The hexagon in the positions immediately to the left and right of the active persona:
  - [ ] Are a bit smaller than the central hexagon.
  - [ ] Have a small amount of blur.
- The hexagon in the positions two to the left and right of the active persona:
  - [ ] Are a much smaller than the central hexagon.
  - [ ] Have a medium amount of blur.

#### Unit 1.1.3: Correct display of the persona information.

**Priority**: High

**Test Description**:
Making sure that the UI is displaying the correct persona information.

**Dependencies**:
- Unit 1.1.1, as you need to be able to navigate the UI to see the information.

**Expected Output**:
- [ ] Each hexagon has the correct icon
- [ ] Each hexagon has the correct name
- [ ] Each hexagon has the correct description

---

## Featureset 2 Wallet System

### Feature 2.1 Login Screen

This is the screen that the user sees when they first open the app. It allows them to login to their Emergence account by scanning a QR code with their wallet app.

#### Unit 2.1.1: Basic functionality of the login screen.

**Priority**: High

**Test Description**:
Making sure that the UI is displayed correctly and that the user can login to their Emergence account.

**Expected Output**:
- [ ] The user can see a QR code on the screen when they open the overlay.
- [ ] The user can scan the QR code with their wallet app and get a sign in request.
- [ ] The user can sign the request and be logged in to their Emergence account.
- [ ] If the user declines the sign in request the QR code is refreshed.
- [ ] The QR code is refreshed every 60 seconds.
- [ ] The QR code time to refresh is displayed on the screen under the QR code.

---

## Featureset 3 Sample Project

### Feature 3.1 General Features

These are the features of the sample project (other than the interactable stations).

#### Unit 3.1.1: Open Overlay via keybind

**Priority**: High

**Test Description**:
Checking that the overlay can be opened via a keybind

**Required Inputs**:
- Pressing the Z key

**Expected Output**:
- [ ] The Emergence overlay opens. If the user has never logged in using the overlay in this project, it should show the onboarding slides, otherwise, it should go straight to the QR code screen.

#### Unit 3.1.2: VRM Avatar Loading

**Priority**: High

**Test Description**:
Checking the player's character model is changed when a persona is loaded.

**Required Inputs**:
- User is already signed into Emergence
- User has a persona
- User's persona has an EAS avatar

**Expected Output**:
- [ ] User's character model changes to be the avatar associated with the persona.

### Feature 3.2 Example Stations

These are the interactable stations, which each have button(s) to perform some action as part of an interactive example 

#### Unit 3.2.1: Open Overlay Station

**Priority**: High

**Test Description**:
Making sure the station works correctly by allowing the user to open the overlay.

**Required Inputs**:
- The user is close enough to the button (within about a metre of the button).

**Expected Output**:
- [ ] Each button says what method they call on them at the bottom.
- [ ] When the user looks at the button, the HUD displays "Press E" to interact.
- [ ] The text on the station should tell the user what the button does, where they can find the code that the button activates, and why they'd use the code in their game.
- [ ] When the button is pressed, the overlay opens. If the user has never logged in using the overlay in this project, it should show the onboarding slides, otherwise, it should go straight to the QR code screen.

#### Unit 3.2.2: Request to Sign Message Station

**Priority**: High

**Test Description**:
Making sure the station works by sending a message to be signed to the users wallet.

**Required Inputs**:
- The user is close enough to the button they wish to make use of.

**Expected Output**:
- [ ] The button says what method they call on them at the bottom.
- [ ] When the user looks at the button, the HUD displays "Press E" to interact.
- [ ] The text on the station should tell the user what the button does, where they can find the code that the button activates, and why they'd use the code in their game.
- [ ] When the request signed message button is pressed, it first checks if the user has logged into the overlay. If they haven't it displays "Make sure your walletconnect'd (press "z") before trying to use this sample!" to the user.
- [ ] If the user is logged in, when they press the button a request to sign message should be sent to the user's wallet. When this is signed, the returned message should be checked for validity using "ValidateSignedMessage" (this isn't technically required, but is to show the feature to new users). Then, the signed message is shown to the user.

#### Unit 3.2.2: Reading and Writing To Smart Contracts Station

**Priority**: High

**Test Description**:
Making sure the station works correctly by allowing the user to read and write from the simple smart contract.

**Required Inputs**:
- The user is close enough to the button they wish to make use of.

**Expected Output**:
- [ ] Each button says what method they call on them at the bottom.
- [ ] When the user looks at the button, the HUD displays "Press E" to interact.
- [ ] The text on the station should tell the user what the button does, where they can find the code that the button activates, and why they'd use the code in their game.
- [ ] It shouldn't be possible to use both buttons at once.
- [ ] When either the GetCurrentCount button or the IncrementCount button is pressed, it first checks if the user has logged into the overlay. If they haven't it displays "Make sure your walletconnect'd (press "z") before trying to use this sample!" to the user.
- [ ] If the user has logged in before pressing the GetCurrentCount button, it calls the "GetCurrentCount" method via ReadMethod and displays "Calling GetCurrentCount on 0xC9571AaF9EbCa8C08EC37D962310d0Ab9F8A5Dd2" to the user.
- [ ] Once the GetCurrentCount async request comes back from the ReadMethod, it outputs the number in the response to the user.
- [ ] If the user has logged in before pressing the IncrementCount button, it calls the "IncrementCount" method via WriteMethod and displays "Calling GetCurrentCount on 0xC9571AaF9EbCa8C08EC37D962310d0Ab9F8A5Dd2. Check your wallet for the transaction confirmation message!" to the user. If the user isn't on the Goerli network, they should recieve a message asking them to switch chain first, then the accept transaction message should show in their wallet.
- [ ] When the user signs the transaction in their wallet, "WalletConnect'd wallet confirmed!" is displayed to the user.
- [ ] When the transaction has been confirmed, it displays "Transaction Hash: {hash}" to the user, where {hash} is the transaction hash.
- [ ] After using the IncrementCount button, using the GetCurrentCount button again should give a higher number than it did previously.

#### Unit 3.2.3: Emergence Avatars Station

**Priority**: High

**Test Description**:
Making sure the station works correctly by allowing the user to mint an EAS avatar.

**Required Inputs**:
- The user is close enough to the button (within about a metre of the button).

**Expected Output**:
- [ ] When the user looks at the button, the HUD displays "Press E" to interact.
- [ ] The text on the station should tell the user what the button does, where they can find the code that the button activates, and why they'd use the code in their game.
- [ ] When the mint avatar button is pressed, it first checks if the user has logged into the overlay. If they haven't it displays "Make sure your walletconnect'd (press "z") before trying to use this sample!" to the user.
- [ ] If the user has logged in before pressing the mint avatar button, it calls the "mint" method via WriteMethod and displays "Calling Mint on 0x074534df6174759a5ae3ad81e3bcdfc0f940f6a6. Check your wallet for the transaction confirmation message!" to the user. If the user isn't on the Polygon network, they should recieve a message asking them to switch chain first, then the accept transaction message should show in their wallet.
- [ ] If the user is already on the Polygon network, the accept transaction message should show in their wallet without a switch chain message / confirmation.
- [ ] When the user signs the transaction in their wallet, "WalletConnect'd wallet confirmed!" is displayed to the user.
- [ ] When the transaction has been confirmed, it displays "Transaction Hash: {hash}" to the user, where {hash} is the transaction hash.
- [ ] Once the transaction has been confirmed, the user should have a new avatar in their wallet.

#### Unit 3.2.4: Inventory Service Station

**Priority**: High

**Test Description**:
Making sure the station works correctly by showing the user an example of an in-game inventory.

**Required Inputs**:
- The user is close enough to the button (within about a metre of the button).

**Expected Output**:
- [ ] When the user looks at the button, the HUD displays "Press E" to interact.
- [ ] The text on the station should tell the user what the button does, where they can find the code that the button activates, and why they'd use the code in their game.
- [ ] When the in-game inventory button is pressed, it first checks if the user has logged into the overlay. If they haven't it displays "Make sure your walletconnect'd (press "z") before trying to use this sample!" to the user.
- [ ] If the user has logged in before pressing the in-game inventory button, it opens an example of an in-game inventory. This shouldn't cover the whole screen, and should display NFTs from the logged-in users wallet. It doesn't need to be interactable.
- [ ] If the button is pressed after the inventory has been opened, then it closes the inventory.

#### Unit 3.2.5: Dynamic Metadata Station

**Priority**: High

**Test Description**:
Making sure the station works correctly by opening the NFT picker, and incrementing a counter in the dynamic metadata of the selected NFT.

**Required Inputs**:
- The user is close enough to the button (within about a metre of the button).

**Expected Output**:
- [ ] When the user looks at the button, the HUD displays "Press E" to interact.
- [ ] The text on the station should tell the user what the button does, where they can find the code that the button activates, and why they'd use the code in their game.
- [ ] When the in-game inventory button is pressed, it first checks if the user has logged into the overlay. If they haven't it displays "Make sure your walletconnect'd (press "z") before trying to use this sample!" to the user.
- [ ] If the user has logged in before pressing the dynamic metadata button, it opens the NFT picker with the default filter-set. When the user selects an NFT with this picker, the message "Current Dynamic Metadata is: {0}" is displayed, where {0} is the current dynamic metadata of this NFT. Then it displays the message "Network: {n} Contract: {c} Token ID: {t}", where {n} is the blockchain the NFT is on, {c} is the contract address, and {t} is the token ID. Then, it calls WriteDynamicMetadata on this NFT, where if the existing dynamic metdata contains just an interger, it increments that interger by 1 and then writes the result of that to the metadata. If it contains any other metadata, it overwrites this metadata with the interger 1. After this has happened, it writes "New Dynamic Metadata is: {0}" where {0} is the new dynamic metadata
- [ ] If the output of WriteDyanmicMetadata is an error, it writes the error code to the output.

---

This test plan is structured by featuresets, each containing multiple features, and each feature containing multiple units. For each unit, the following information is provided:

- **Priority**: Level of importance ([High/Medium/Low]).
    - **High**: Should be tested if the change touched this featureset.
    - **Medium**: Should be tested if the change specifically altered this feature.
    - **Low**: Should be tested if the change touched this particular unit.

- **Test Description**: Explanation of [what is being tested and why].
- **Dependencies**: Any preconditions or tests that must be completed successfully before this test. [None or list dependencies]
- **Required Inputs**: Any input data needed for the test. [Input 1, Input 2, ...]
- **Expected Output**: Description of the expected result of the test. [Output description]

### Template Unit

#### Unit x.y.z: [Name]

**Priority**: [High/Medium/Low]

**Test Description**:
- [What we are testing and why.]

**Glossary**
[Optional glossary of terms used in this unit.]
- [Term 1]: [Definition] 

**Dependencies**:
- [Must complete Unit 1.1.1 successfully or other dependencies.]

**Required Inputs**:
- [Input 1]
- [Input 2]

**Expected Output**:
- [ ] [Output description]