import {id} from './util.js'

export type PlayerId = string

export class Player {
  id: PlayerId
  index: number = -1
  name: string
  public password = ''
  public email = ''
  gameId: number = -1

  constructor(name: string) {
    this.id = id()
    this.name = name
  }
}
