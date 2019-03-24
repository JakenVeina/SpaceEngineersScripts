# Door Manager

This script manages doors in your ships and stations, by ensuring that they automatically close after a configured amount of time, and by providing functionality to lock doors and prevent them from being opened at all.

## Commands

This script supports the following commands (I.E. arguments):

- `reload`
  - Reloads configuration settings from "Custom Data"
  - Re-maps blocks to be managed by the script
- `run`
  - Initiates continuous execution of this script, in the background
- `stop`
  - Stops continuous execution of this script, in the background
- `lockdown`
  - Enables "lockdown" mode for all "managed" doors, where every door is immediately closed, and then disabled, to prevent them from being opened by players.
- `release`
  - Disables "lockdown" mode, as described above
- `stats`
  - Displays information about the current state of the script, including the number of doors currently being managed.
 
 ## Script Configuration
 
 This script supports the following configuration settings, applied through the "Custom Data" setting of the host Programmable Block. Each setting should be written on a separate line of the "Custom Data" text.
 
- `block-tag:[BlockTag]`
  - Defines the tag to be used to apply block-specific settings to blocks managed by this script (see below).
  - Defaults to "DoorManager"
- `auto-manage-this-grid`
  - Instructs this script to automatically select "managed" blocks from the same grid as the host Programmable Block
- `manage-other-grids`
  - Instructs this script to allow for blocks not upon the same grid as the host Programmable Block to be "managed"
- `auto-manage-other-grids`
  - Instructs this script to automatically select "managed" blocks from grids other than the grid of the host Programmable Block
- `manage-interval:[ManageInterval (> 0) (ms)]`
  - Sets how often the script "manages" each door
  - Defaults to 500
- `auto-close-interval:[AutoCloseInterval (> 0) (ms)]`
  - Sets how long a "managed" door can remain open, before it is automatically closed
  - Defaults to 3000
- `max-log-size:[MaxLogSize (>= 0) (lines)]`
  - Sets the maximum number of lines to be kept at once, in the text log of the host Programmable Block
  - Defaults to 10
- `instructions-per-tick:[InstructionsPerTick (> 0)]`
  - Sets the target maximum number of instructions to be executed by the script, within a single game tick
  - When a script operation reaches this threshold without completing, it will automatically re-trigger the Programmable Block and continue on the next tick.
  - Defaults to 1000

## Block Configuration

Additionally, this script supports the following configuration settings upon blocks capable of being "managed". Each of these settings must be applied with a [BlockTag] value matching the "block-tag" setting of the host Programmable Block.

- `[BlockTag]:manage`
  - Instructs the script to recognize this block as "managed", if it has not already been selected as automatically managed by the script.
- `[BlockTag]:ignore`
  - Instructs the script to not recognize this block as "managed", even if has already been selected as automatically managed by the script.
  - `[BlockTag]:auto-close-interval:[AutoCloseInterval (> 0) (ms)]`
  - Sets how long this particular "managed" door can remain open, before it is automatically closed. If specified, this setting supercedes the `auto-close-interval` setting specified at the script level, upon the host Programmable Block