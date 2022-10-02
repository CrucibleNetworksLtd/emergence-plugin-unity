using EmergenceSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tests : MonoBehaviour
{
    private static void Test()
    {
        BlockchainService.Instance.GetBlockNumber(LocalEmergenceServer.Instance.Environment().defaultNodeURL, (GetBlockNumberResponse blockNumber) =>
        {
            Debug.Log(blockNumber.blockNumber);
        }, (err, code) => { });

        //Services.Instance.ValidateSignedMessage("Hello World!", "0x022ca9b04985227af4ca9763161c5d38ad6f6356c1df0e30f8c2352416e0ed082035e4b9b785adb7d49932af2b625c028cdb4a85b8ce40ba2d475d37f888fd7a1c", "0x8eaeA2307a0fA0D7F78Af416554B3653eDE2571B", (res) =>
        //{

        //}, (err, code) => { });


        //Services.Instance.GetTransactionStatus("0xa4828a2c6adef3a1f4212a1e66b2924db7f3d87bd4070e10135da95ec80b1c5a", LocalEmergenceServer.Instance.Environment().defaultNodeURL, (GetTransactionStatusResponse res) =>
        //{

        //}, (error, code) =>
        //{

        //});

        //    Services.Instance.AccessToken =
        //        "0xd521344ebaf5d166eac916762579740c743a9e95660823862ac036880dc2ee5e5ee9d724f0e2a24514694b132d15fe2cec89e7d2298f893f1fe12eae8c7888d01c";

        //    Services.Instance.ValidateAccessToken((res) =>
        //    {

        //    }, (erro, code) =>
        //    {

        //    });

        //    Services.Instance.GetPersonas((personas, currentPersona) =>
        //    {

        //    }, (err, code) => { });


        BlockchainService.Instance.LoadContract("0x7C8e7F9310De0545567E3a5d0097Fcec67B1EDf4", "[	{		\"inputs\": [],		\"stateMutability\": \"nonpayable\",		\"type\": \"constructor\"	},	{		\"anonymous\": false,		\"inputs\": [			{				\"indexed\": true,				\"internalType\": \"address\",				\"name\": \"owner\",				\"type\": \"address\"			},			{				\"indexed\": true,				\"internalType\": \"address\",				\"name\": \"approved\",				\"type\": \"address\"			},			{				\"indexed\": true,				\"internalType\": \"uint256\",				\"name\": \"tokenId\",				\"type\": \"uint256\"			}		],		\"name\": \"Approval\",		\"type\": \"event\"	},	{		\"anonymous\": false,		\"inputs\": [			{				\"indexed\": true,				\"internalType\": \"address\",				\"name\": \"owner\",				\"type\": \"address\"			},			{				\"indexed\": true,				\"internalType\": \"address\",				\"name\": \"operator\",				\"type\": \"address\"			},			{				\"indexed\": false,				\"internalType\": \"bool\",				\"name\": \"approved\",				\"type\": \"bool\"			}		],		\"name\": \"ApprovalForAll\",		\"type\": \"event\"	},	{		\"inputs\": [			{				\"internalType\": \"address\",				\"name\": \"to\",				\"type\": \"address\"			},			{				\"internalType\": \"uint256\",				\"name\": \"tokenId\",				\"type\": \"uint256\"			}		],		\"name\": \"approve\",		\"outputs\": [],		\"stateMutability\": \"nonpayable\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"uint256\",				\"name\": \"tokenId\",				\"type\": \"uint256\"			}		],		\"name\": \"burn\",		\"outputs\": [],		\"stateMutability\": \"nonpayable\",		\"type\": \"function\"	},	{		\"inputs\": [],		\"name\": \"increase\",		\"outputs\": [],		\"stateMutability\": \"payable\",		\"type\": \"function\"	},	{		\"anonymous\": false,		\"inputs\": [			{				\"indexed\": true,				\"internalType\": \"address\",				\"name\": \"previousOwner\",				\"type\": \"address\"			},			{				\"indexed\": true,				\"internalType\": \"address\",				\"name\": \"newOwner\",				\"type\": \"address\"			}		],		\"name\": \"OwnershipTransferred\",		\"type\": \"event\"	},	{		\"inputs\": [],		\"name\": \"renounceOwnership\",		\"outputs\": [],		\"stateMutability\": \"nonpayable\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"address\",				\"name\": \"to\",				\"type\": \"address\"			}		],		\"name\": \"safeMint\",		\"outputs\": [],		\"stateMutability\": \"nonpayable\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"address\",				\"name\": \"from\",				\"type\": \"address\"			},			{				\"internalType\": \"address\",				\"name\": \"to\",				\"type\": \"address\"			},			{				\"internalType\": \"uint256\",				\"name\": \"tokenId\",				\"type\": \"uint256\"			}		],		\"name\": \"safeTransferFrom\",		\"outputs\": [],		\"stateMutability\": \"nonpayable\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"address\",				\"name\": \"from\",				\"type\": \"address\"			},			{				\"internalType\": \"address\",				\"name\": \"to\",				\"type\": \"address\"			},			{				\"internalType\": \"uint256\",				\"name\": \"tokenId\",				\"type\": \"uint256\"			},			{				\"internalType\": \"bytes\",				\"name\": \"data\",				\"type\": \"bytes\"			}		],		\"name\": \"safeTransferFrom\",		\"outputs\": [],		\"stateMutability\": \"nonpayable\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"address\",				\"name\": \"operator\",				\"type\": \"address\"			},			{				\"internalType\": \"bool\",				\"name\": \"approved\",				\"type\": \"bool\"			}		],		\"name\": \"setApprovalForAll\",		\"outputs\": [],		\"stateMutability\": \"nonpayable\",		\"type\": \"function\"	},	{		\"anonymous\": false,		\"inputs\": [			{				\"indexed\": true,				\"internalType\": \"address\",				\"name\": \"from\",				\"type\": \"address\"			},			{				\"indexed\": true,				\"internalType\": \"address\",				\"name\": \"to\",				\"type\": \"address\"			},			{				\"indexed\": true,				\"internalType\": \"uint256\",				\"name\": \"tokenId\",				\"type\": \"uint256\"			}		],		\"name\": \"Transfer\",		\"type\": \"event\"	},	{		\"inputs\": [			{				\"internalType\": \"address\",				\"name\": \"from\",				\"type\": \"address\"			},			{				\"internalType\": \"address\",				\"name\": \"to\",				\"type\": \"address\"			},			{				\"internalType\": \"uint256\",				\"name\": \"tokenId\",				\"type\": \"uint256\"			}		],		\"name\": \"transferFrom\",		\"outputs\": [],		\"stateMutability\": \"nonpayable\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"address\",				\"name\": \"newOwner\",				\"type\": \"address\"			}		],		\"name\": \"transferOwnership\",		\"outputs\": [],		\"stateMutability\": \"nonpayable\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"address\",				\"name\": \"owner\",				\"type\": \"address\"			}		],		\"name\": \"balanceOf\",		\"outputs\": [			{				\"internalType\": \"uint256\",				\"name\": \"\",				\"type\": \"uint256\"			}		],		\"stateMutability\": \"view\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"uint256\",				\"name\": \"tokenId\",				\"type\": \"uint256\"			}		],		\"name\": \"getApproved\",		\"outputs\": [			{				\"internalType\": \"address\",				\"name\": \"\",				\"type\": \"address\"			}		],		\"stateMutability\": \"view\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"address\",				\"name\": \"owner\",				\"type\": \"address\"			},			{				\"internalType\": \"address\",				\"name\": \"operator\",				\"type\": \"address\"			}		],		\"name\": \"isApprovedForAll\",		\"outputs\": [			{				\"internalType\": \"bool\",				\"name\": \"\",				\"type\": \"bool\"			}		],		\"stateMutability\": \"view\",		\"type\": \"function\"	},	{		\"inputs\": [],		\"name\": \"name\",		\"outputs\": [			{				\"internalType\": \"string\",				\"name\": \"\",				\"type\": \"string\"			}		],		\"stateMutability\": \"view\",		\"type\": \"function\"	},	{		\"inputs\": [],		\"name\": \"owner\",		\"outputs\": [			{				\"internalType\": \"address\",				\"name\": \"\",				\"type\": \"address\"			}		],		\"stateMutability\": \"view\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"uint256\",				\"name\": \"tokenId\",				\"type\": \"uint256\"			}		],		\"name\": \"ownerOf\",		\"outputs\": [			{				\"internalType\": \"address\",				\"name\": \"\",				\"type\": \"address\"			}		],		\"stateMutability\": \"view\",		\"type\": \"function\"	},	{		\"inputs\": [],		\"name\": \"state\",		\"outputs\": [			{				\"internalType\": \"uint256\",				\"name\": \"\",				\"type\": \"uint256\"			}		],		\"stateMutability\": \"view\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"bytes4\",				\"name\": \"interfaceId\",				\"type\": \"bytes4\"			}		],		\"name\": \"supportsInterface\",		\"outputs\": [			{				\"internalType\": \"bool\",				\"name\": \"\",				\"type\": \"bool\"			}		],		\"stateMutability\": \"view\",		\"type\": \"function\"	},	{		\"inputs\": [],		\"name\": \"symbol\",		\"outputs\": [			{				\"internalType\": \"string\",				\"name\": \"\",				\"type\": \"string\"			}		],		\"stateMutability\": \"view\",		\"type\": \"function\"	},	{		\"inputs\": [			{				\"internalType\": \"uint256\",				\"name\": \"tokenId\",				\"type\": \"uint256\"			}		],		\"name\": \"tokenURI\",		\"outputs\": [			{				\"internalType\": \"string\",				\"name\": \"\",				\"type\": \"string\"			}		],		\"stateMutability\": \"view\",		\"type\": \"function\"	}]", () =>
        {
            BlockchainService.Instance.WriteContract("0x7C8e7F9310De0545567E3a5d0097Fcec67B1EDf4", "increase", "", "", "", (WriteContractResponse writeres) =>
            {
                Debug.Log("Write OK: " + writeres.transactionHash);
                BlockchainService.Instance.ReadContract("0x7C8e7F9310De0545567E3a5d0097Fcec67B1EDf4", "state", "", (ReadContractResponse readres) =>
                {
                    Debug.Log("Read OK: " + readres.response);
                }, (err, status) =>
                {
                    Debug.LogError($"{err}: {status}");
                });
            }, (err, status) =>
            {
                Debug.LogError($"{err}: {status}");
            });

        }, (err, code) => { });



    }

    #region Plumbing
    void Start()
    {
        Wait(new WaitUntil(() => { return LocalEmergenceServer.Instance.IsStarted(); }), () =>
        {
            Test();

        });
    }

    private void Wait(WaitUntil waitUntil, Action p)
    {
        StartCoroutine(WaitCo(waitUntil, p));
    }

    private IEnumerator WaitCo(WaitUntil waitUntil, Action p)
    {
        yield return waitUntil;
        p();

    }

    #endregion
}
