# Perkify

Perkify is a minimalist kernel written in C#, designed for the abstraction and underlying implementation of core functionalities related to various types of coupons, prepaid cards, subscription and membership.

## Architecture

The overall architecture of Perkify is designed to be modular, extensible and flexible, with a focus on the separation of concerns and the ease of integration with other systems.

![Perkify Architecture](./doc/perkify.architecture.png)]

## Features

We are currently working on the Perkify Core, which is the runtime kernel of the system.

Here are the core modules and interfaces that we have implemented so far:
- [x] **Eligible**: The general eligibility check interface for any specific coupon, prepaid card, subscription or membership.
- [x] **Balance**: The balance management for coupon or credits (prepaid card).
- [x] **Expiry**: The expiry management for coupon, subscription or credits (prepaid card).
- [x] **Enablement**: The enablement management for coupon, subscription or credits (prepaid card).
- [x] **Delegation**: The flexible delegation-based eligiblity check for better extensibiity.
- [x] **Entitlement**: Combination of eligibility check with balance, expiry and enablement.
- [x] **Chain**: The chain of entitlements with specified order (expiry time by default).
- [ ] **Taxonomy**: Define a tags system and run eligibility check with the tags in flexible way.
