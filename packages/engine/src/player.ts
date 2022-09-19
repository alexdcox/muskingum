import {id} from './util.js'

export type PlayerId = string

export class Player {
  id: PlayerId
  name: string
  public password = ''
  public email = ''

  constructor(name: string) {
    this.id = id()
    this.name = name
  }
}
