# Git Hands-On Session

**Format:** Demo → You try it  
**Duration:** ~3 hours  
**Platform:** Gitea (`devops-01.marinha.pt`) + your local machine

---

## Before the session

Make sure every participant has:

- A laptop with internet access
- A Gitea account on `devops-01.marinha.pt`
- Access to the `pessoas-integracao` repository (ask a team lead)
- VS Code installed (recommended, not mandatory)

The facilitator should have the repo open on a projected screen and walk through each topic live before participants try it themselves.

---

## Agenda

| #   | Topic                                                    | Format     | Time   |
| --- | -------------------------------------------------------- | ---------- | ------ |
| 1   | [Install Git](./01-install-git.md)                       | Demo → Try | 15 min |
| 2   | [Configure SSH Keys](./02-ssh-keys.md)                   | Demo → Try | 25 min |
| 3   | [Clone a Repo](./03-clone.md)                            | Demo → Try | 15 min |
| 4   | [GitHub Flow](./04-github-flow.md)                       | Demo → Try | 25 min |
| 5   | [Conventional Branching](./05-conventional-branching.md) | Demo → Try | 20 min |
| 6   | [Semantic Commits](./06-semantic-commits.md)             | Demo → Try | 20 min |
| 7   | [Create a PR](./07-create-pr.md)                         | Demo → Try | 25 min |
| –   | Wrap-up & Q&A                                            | Discussion | 10 min |

---

## How it works

Each topic follows the same structure:

```
📖 Facilitator explains the concept (5–10 min)
   ↓
💻 Everyone tries it on their own machine (10–15 min)
   ↓
🙋 Questions and clarifications
   ↓
➡️  Next topic
```

If you get stuck during the "try it" phase, raise your hand. Don't copy-paste — type the commands yourself so they stick.

---

## The exercise repo

Throughout the session everyone will work on a **fork** of `pessoas-integracao`. By the end of the session, each participant will have:

- Their own fork on Gitea
- A local clone on their laptop
- A feature branch with a real commit
- An open Pull Request against the upstream repo
