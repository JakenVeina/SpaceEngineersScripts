
# Space Engineers Scripts

A library of in-game scripts for Space Engineers

## Automated Docking Procedures

This script automates the process of enabling, disabling, or reconfiguring blocks when docking or undocking ships from each other, or from a station.

### Commands

This script supports the following commands (I.E. arguments):

- `reload`
  - Reloads configuration settings from "Custom Data"
  - Re-maps blocks to be managed by the script
- `dock`
  - From among "managed" blocks...
    - Connects Connectors currently in range
    - Locks Landing Gears currently in range
    - Disables the following types of blocks
      - Beacons
      - Gas Generators
      - Gyroscopes
      - Lights
      - Radio Antennae
      - Reactors
      - Thrusters
    - Sets all Batteries to "Recharge" mode
    - Enables "Stockpile" mode on all Gas Tanks
- `undock`
  - Reverses all operations performed by the "dock" command
 
 ### Script Configuration
 
 This script supports the following configuration settings, applied through the "Custom Data" setting of the host Programmable Block. Each setting should be written on a separate line of the "Custom Data" text.
 
- `block-tag:[BlockTag]`
  - Defines the tag to be used to apply block-specific settings to blocks managed by this script (see below).
  - Defaults to "AutomatedDocking"
- `auto-manage-this-grid`
  - Instructs this script to automatically select "managed" blocks from the same grid as the host Programmable Block
- `manage-other-grids`
  - Instructs this script to allow for blocks not upon the same grid as the host Programmable Block to be "managed"
- `auto-manage-other-grids`
  - Instructs this script to automatically select "managed" blocks from grids other than the grid of the host Programmable Block
- `ignore-battery-blocks`
  - Instructs this script to not select Battery Blocks as "managed"
- `ignore-beacons`
  - Instructs this script to not select Beacons as "managed"
- `ignore-gas-generators`
  - Instructs this script to not select Gas Generators as "managed"
- `ignore-gas-tanks`
  - Instructs this script to not select Gas Tanks as "managed"
- `ignore-gyros`
  - Instructs this script to not select Gyroscopes as "managed"
- `ignore-landing-gears`
  - Instructs this script to not select Landing Gears as "managed"
- `ignore-lighting-blocks`
  - Instructs this script to not select Lighting Blocks as "managed"
- `ignore-radio-antennae`
  - Instructs this script to not select Radio Antenna Blocks as "managed"
- `ignore-reactors`
  - Instructs this script to not select Reactors as "managed"
- `ignore-thrusters`
  - Instructs this script to not select Thrusters as "managed"
- `max-log-size:[MaxLogSize (>= 0) (lines)]`
  - Sets the maximum number of lines to be kept at once, in the text log of the host Programmable Block
- `instructions-per-tick:[InstructionsPerTick (> 0)]`
  - Sets the target maximum number of instructions to be executed by the script, within a single game tick
  - When a script operation reaches this threshold without completing, it will automatically re-trigger the Programmable Block and continue on the next tick.

### Block Configuration

Additionally, this script supports the following configuration settings upon blocks capable of being "managed". Each of these settings must be applied with a [BlockTag] value matching the "block-tag" setting of the host Programmable Block.

- `[BlockTag]:manage`
  - Instructs the script to recognize this block as "managed", if it has not already been selected as automatically managed by the script.
- `[BlockTag]:ignore`
  - Instructs the script to not recognize this block as "managed", even if has already been selected as automatically managed by the script.