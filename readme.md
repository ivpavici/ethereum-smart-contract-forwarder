- Problem:
	https://ethereum.stackexchange.com/questions/84401/how-to-build-an-ethereum-hot-wallet-for-an-exchange

- Goals:
	- Develop a POC application that will handle creation of ethereum deposit addresses for user accounts, with help from smart contract forwarders, that automatically (and without paying gas) consolidate ethereum on a hot wallet. This can be used for example in a cryptocurrency exchange.
	- The app will be a .Net 5 web api application, which can be connected to any frontend.
	- Create a minimal react or vue.js app to call APIs
	- Use TDD principles while developing

- prerequisites:
	- Use Infura test chain
	- Or locally:
	1. Testchain: https://github.com/Nethereum/Testchains
		- I used Geth for Windows testchain for local development, just donwload the project and run the bath script
		- Commands to interact with the CLI  https://ethereum.stackexchange.com/questions/28703/full-list-of-geth-terminal-commands/28704
		- Example command in CLI: web3.fromWei(eth.getBalance('0x13f022d72158410433cbd66f5dd8bf6d2d129924')); 
	2. https://github.com/Nethereum/Nethereum.WalletForwarder/tree/main/testchain

- To run: 
 	- docker compose up -d
 	- run migrations
 	- in postman/insomnia reach the endpoint via (for exmample) http://localhost:5000/api/assets
