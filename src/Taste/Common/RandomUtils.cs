/*
 * Copyright 2007 and onwards Sean Owen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Taste.Common
{
	using System;


	/**
	 * @author Sean Owen
	 */
	public sealed class RandomUtils 
    {
		private const int STANDARD_SEED = 0xBADCAFE;
		private static bool testSeed;

		private RandomUtils() {}

		public static void UseTestSeed() 
		{
			testSeed = true;
		}

		public static Random GetRandom() 
		{
			return testSeed ? new Random(STANDARD_SEED) : new Random();
		}
	}
}